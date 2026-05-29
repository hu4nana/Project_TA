using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerInput))]
public class Player : Character
{
    [Header("Jump Assist")]
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float groundIgnoreAfterJump = 0.12f;

    [Header("Wall Check")]
    [SerializeField] float wallCheckDistance = 0.08f;
    [SerializeField] Vector2 wallCheckSize = new(0.12f, 1.6f);

    PlayerInputReader inputReader;
    PlayerMotor motor;
    PlayerCombat combat;
    PlayerDefense defense;
    PlayerInteractor interactor;
    PlayerResourceController resources;
    PlayerHitReceiver hitReceiver;
    PlayerSkillCaster skillCaster;

    bool wasGrounded;
    bool isTouchingWall;
    int wallDirection;
    float coyoteTimer;
    float groundIgnoreTimer;
    bool inputLocked;

    public int MaxAirJumpCount => Mathf.Max(0, maxJumpChance - 1);
    public int AirJumpRemaining => jumpChance;

    private void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        inputReader = GetRequiredInChildren<PlayerInputReader>();
        motor = GetRequiredInChildren<PlayerMotor>();
        combat = GetRequiredInChildren<PlayerCombat>();
        defense = GetRequiredInChildren<PlayerDefense>();
        interactor = GetRequiredInChildren<PlayerInteractor>();
        resources = GetRequiredInChildren<PlayerResourceController>();
        hitReceiver = GetRequiredInChildren<PlayerHitReceiver>();
        GetRequiredInChildren<PlayerSkillLoadout>();
        GetRequiredInChildren<PlayerFeedbacks>();
        skillCaster = GetRequiredInChildren<PlayerSkillCaster>();
        jumpChance = MaxAirJumpCount;

        if (motor == null || hitReceiver == null || defense == null || resources == null)
            return;

        motor.Initialize(this);
        hitReceiver.Initialize(this, defense, resources, motor);
    }

    void Update()
    {
        UpdateGroundState(Time.deltaTime);
        UpdateWallState();
        resources.Tick(Time.deltaTime);
        combat.Tick(this, Time.deltaTime);
        defense.Tick(this, Time.deltaTime);
        hitReceiver.Tick(Time.deltaTime);
        skillCaster.Tick(this, resources, Time.unscaledDeltaTime);

        if (conditionState == ConditionState.Dead)
            return;

        if (inputLocked)
        {
            if (inputReader.HasAttackPressed)
            {
                DialogueController.Instance?.Advance();
                inputReader.ConsumeAttackPressed();
            }

            inputReader.TickBuffers();
            motor.StopHorizontalMovement();
            ChangeState(MovementState.Idle);
            return;
        }

        HandleActions();

        if (inputReader.ConsumeJumpReleased())
            motor.CutJump();

        inputReader.TickBuffers();
        UpdateMovementState();
    }

    void FixedUpdate()
    {
        if (conditionState == ConditionState.Dead)
            return;

        if (inputLocked)
        {
            motor.StopHorizontalMovement();
            motor.Tick(Time.fixedDeltaTime, inputReader.JumpHeld);
            return;
        }

        float moveX = inputReader.Move.x;
        bool pushingIntoWall = IsPushingIntoWall(moveX);

        if (actionState != ActionState.Parry && !pushingIntoWall)
        {
            motor.Move(moveX);
        }
        else if (pushingIntoWall)
        {
            motor.StopHorizontalMovement();
        }

        motor.Tick(Time.fixedDeltaTime, inputReader.JumpHeld);
    }

    void HandleActions()
    {
        if (inputReader.HasParryPressed && defense.TryStartParry(this))
            inputReader.ConsumeParryPressed();

        if (inputReader.HasDashPressed && defense.TryStartDodge(this))
        {
            motor.StartDodge(defense.DodgeDashDuration);
            inputReader.ConsumeDashPressed();
        }

        if (inputReader.HasJumpPressed && TryJump())
            inputReader.ConsumeJumpPressed();

        if (inputReader.HasAttackPressed && !skillCaster.IsCasting && combat.TryStartAttack(this))
            inputReader.ConsumeAttackPressed();

        if (inputReader.TryGetSkillPressed(out int skillIndex) && skillCaster.TryCast(this, resources, skillIndex))
            inputReader.ConsumeSkillPressed();

        if (inputReader.HasInteractPressed && interactor.TryInteract(this))
            inputReader.ConsumeInteractPressed();
    }

    void UpdateMovementState()
    {
        if (!isGrounded)
        {
            ChangeState(MovementState.Jump);
            return;
        }

        if (Mathf.Abs(rigid.linearVelocity.x) > 0.05f || Mathf.Abs(inputReader.Move.x) > 0.01f)
            ChangeState(MovementState.Walk);
        else
            ChangeState(MovementState.Idle);
    }

    void UpdateGroundState(float deltaTime)
    {
        bool rawGrounded = Grounded();

        if (groundIgnoreTimer > 0f)
        {
            groundIgnoreTimer -= deltaTime;
            if (rigid.linearVelocity.y > 0.01f)
                rawGrounded = false;
        }

        wasGrounded = isGrounded;
        isGrounded = rawGrounded;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (!wasGrounded)
                jumpChance = MaxAirJumpCount;
        }
        else
        {
            coyoteTimer -= deltaTime;
        }
    }

    bool TryJump()
    {
        bool canGroundJump = isGrounded || coyoteTimer > 0f;

        if (canGroundJump)
        {
            StartJump(PlayerJumpType.Ground);
            return true;
        }

        if (jumpChance > 0)
        {
            jumpChance--;
            StartJump(PlayerJumpType.Air);
            return true;
        }

        return false;
    }

    void StartJump(PlayerJumpType jumpType)
    {
        if (jumpType == PlayerJumpType.Ground)
            jumpChance = MaxAirJumpCount;

        coyoteTimer = 0f;
        isGrounded = false;
        groundIgnoreTimer = groundIgnoreAfterJump;
        motor.Jump(jumpType);
    }

    void UpdateWallState()
    {
        Vector2 origin = col.bounds.center;
        Vector2 leftOrigin = origin + Vector2.left * (col.bounds.extents.x + wallCheckDistance * 0.5f);
        Vector2 rightOrigin = origin + Vector2.right * (col.bounds.extents.x + wallCheckDistance * 0.5f);
        int mask = GroundMask;

        bool leftWall = Physics2D.OverlapBox(leftOrigin, wallCheckSize, 0f, mask) != null;
        bool rightWall = Physics2D.OverlapBox(rightOrigin, wallCheckSize, 0f, mask) != null;

        isTouchingWall = !isGrounded && (leftWall || rightWall);
        wallDirection = leftWall ? -1 : rightWall ? 1 : 0;
    }

    bool IsPushingIntoWall(float inputX)
    {
        if (!isTouchingWall || isGrounded || wallDirection == 0 || Mathf.Abs(inputX) <= 0.01f)
            return false;

        return Mathf.Sign(inputX) == wallDirection;
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        inputReader.ClearTransientInputs();

        if (locked)
        {
            inputReader.SetMove(Vector2.zero);
            motor.StopHorizontalMovement();
            ChangeState(MovementState.Idle);
        }
    }

    T GetRequiredInChildren<T>() where T : Component
    {
        T component = GetComponentInChildren<T>(true);
        if (component == null)
            Debug.LogError($"Player is missing required child component: {typeof(T).Name}", this);

        return component;
    }

    #region InputActions
    public void OnMove(InputValue inputValue)
    {
        inputReader.SetMove(inputValue.Get<Vector2>());
    }

    public void OnJump(InputValue inputValue)
    {
        inputReader.SetJump(inputValue.Get<float>() > 0.5f);
    }

    public void OnAttack(InputValue inputValue)
    {
        inputReader.SetAttack(inputValue.Get<float>() > 0.5f);
    }

    public void OnDash(InputValue inputValue)
    {
        inputReader.SetDash(inputValue.Get<float>() > 0.5f);
    }

    public void OnSkill1(InputValue inputValue)
    {
        inputReader.SetSkill(0, inputValue.Get<float>() > 0.5f);
    }

    public void OnSkill2(InputValue inputValue)
    {
        inputReader.SetSkill(1, inputValue.Get<float>() > 0.5f);
    }

    public void OnSkill3(InputValue inputValue)
    {
        inputReader.SetSkill(2, inputValue.Get<float>() > 0.5f);
    }

    public void OnParry(InputValue inputValue)
    {
        inputReader.SetParry(inputValue.Get<float>() > 0.5f);
    }

    public void OnInteract(InputValue inputValue)
    {
        inputReader.SetInteract(inputValue.Get<float>() > 0.5f);
    }
    #endregion
}
