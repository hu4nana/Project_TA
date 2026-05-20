using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerDefense))]
[RequireComponent(typeof(PlayerInteractor))]
[RequireComponent(typeof(PlayerResourceController))]
[RequireComponent(typeof(PlayerHitReceiver))]
[RequireComponent(typeof(PlayerSkillCaster))]
[RequireComponent(typeof(PlayerSkillLoadout))]
[RequireComponent(typeof(PlayerFeedbacks))]
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

    private void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        inputReader = GetOrAdd<PlayerInputReader>();
        motor = GetOrAdd<PlayerMotor>();
        combat = GetOrAdd<PlayerCombat>();
        defense = GetOrAdd<PlayerDefense>();
        interactor = GetOrAdd<PlayerInteractor>();
        resources = GetOrAdd<PlayerResourceController>();
        hitReceiver = GetOrAdd<PlayerHitReceiver>();
        GetOrAdd<PlayerSkillLoadout>();
        GetOrAdd<PlayerFeedbacks>();
        skillCaster = GetOrAdd<PlayerSkillCaster>();

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
            if (inputReader.ConsumeAttackPressed())
                DialogueController.Instance?.Advance();

            motor.StopHorizontalMovement();
            ChangeState(MovementState.Idle);
            return;
        }

        if (!inputReader.JumpHeld)
            motor.CutJump();

        HandleActions();
        UpdateMovementState();
    }

    void FixedUpdate()
    {
        if (conditionState == ConditionState.Dead)
            return;

        if (inputLocked)
        {
            motor.StopHorizontalMovement();
            motor.Tick(Time.fixedDeltaTime);
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

        motor.Tick(Time.fixedDeltaTime);
    }

    void HandleActions()
    {
        if (inputReader.ConsumeParryPressed())
            defense.TryStartParry(this);

        if (inputReader.ConsumeDashPressed() && defense.TryStartDodge(this))
            motor.StartDodge(defense.DodgeDashDuration);

        if (inputReader.ConsumeJumpPressed())
            TryJump();

        if (inputReader.ConsumeAttackPressed() && !skillCaster.IsCasting)
            combat.TryStartAttack(this);

        if (inputReader.ConsumeSkillPressed(out int skillIndex))
            skillCaster.TryCast(this, resources, skillIndex);

        if (inputReader.ConsumeInteractPressed())
            interactor.TryInteract(this);
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
                jumpChance = maxJumpChance;
        }
        else
        {
            coyoteTimer -= deltaTime;
        }
    }

    void TryJump()
    {
        bool canGroundJump = isGrounded || coyoteTimer > 0f;

        if (canGroundJump)
        {
            jumpChance = Mathf.Max(0, maxJumpChance - 1);
        }
        else if (jumpChance > 0)
        {
            jumpChance--;
        }
        else
        {
            return;
        }

        coyoteTimer = 0f;
        isGrounded = false;
        groundIgnoreTimer = groundIgnoreAfterJump;
        motor.Jump();
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

    T GetOrAdd<T>() where T : Component
    {
        T component = GetComponent<T>();
        return component ? component : gameObject.AddComponent<T>();
    }

    #region InputActions
    public void OnMove(InputValue inputValue)
    {
        inputReader.SetMove(inputValue.Get<Vector2>());
    }

    public void OnJump(InputValue inputValue)
    {
        inputReader.SetJump(inputValue.isPressed);
    }

    public void OnAttack(InputValue inputValue)
    {
        inputReader.SetAttack(inputValue.isPressed);
    }

    public void OnDash(InputValue inputValue)
    {
        inputReader.SetDash(inputValue.isPressed);
    }

    public void OnSkill1(InputValue inputValue)
    {
        inputReader.SetSkill(0, inputValue.isPressed);
    }

    public void OnSkill2(InputValue inputValue)
    {
        inputReader.SetSkill(1, inputValue.isPressed);
    }

    public void OnSkill3(InputValue inputValue)
    {
        inputReader.SetSkill(2, inputValue.isPressed);
    }

    public void OnParry(InputValue inputValue)
    {
        inputReader.SetParry(inputValue.isPressed);
    }

    public void OnInteract(InputValue inputValue)
    {
        inputReader.SetInteract(inputValue.isPressed);
    }
    #endregion
}
