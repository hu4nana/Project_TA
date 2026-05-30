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

    PlayerInput playerInput;
    InputAction jumpAction;
    PlayerInputReader inputReader;
    PlayerMotor motor;
    PlayerCombat combat;
    PlayerDefense defense;
    PlayerInteractor interactor;
    PlayerResourceController resources;
    PlayerHitReceiver hitReceiver;
    PlayerSkillCaster skillCaster;
    PlayerGroundSensor groundSensor;
    PlayerActionController actionController;

    bool inputLocked;

    public int MaxAirJumpCount => Mathf.Max(0, maxJumpChance - 1);
    public int AirJumpRemaining => jumpChance;
    public bool IsInputLocked => inputLocked;

    private void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput != null ? playerInput.actions.FindAction("Jump", false) : null;
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
        groundSensor = new PlayerGroundSensor(this, coyoteTime, groundIgnoreAfterJump, wallCheckDistance, wallCheckSize);

        if (motor == null || hitReceiver == null || defense == null || resources == null)
            return;

        motor.Initialize(this);
        hitReceiver.Initialize(this, defense, resources, motor);
        actionController = new PlayerActionController(new PlayerContext(this, inputReader, motor, combat, defense, interactor, resources, skillCaster));
    }

    void Update()
    {
        UpdateGroundState(Time.deltaTime);
        resources.Tick(Time.deltaTime);
        combat.Tick(this, Time.deltaTime);
        defense.Tick(this, Time.deltaTime);
        hitReceiver.Tick(Time.deltaTime);
        skillCaster.Tick(this, resources, Time.unscaledDeltaTime);
        SyncJumpHeldFromInputAction();

        if (conditionState == ConditionState.Dead)
            return;

        if (!inputReader.JumpHeld)
            motor.CutJump();

        if (IsActionBlockedByCondition())
        {
            inputReader.ClearTransientInputs();
            UpdateMovementState();
            return;
        }

        if (inputLocked)
        {
            if (inputReader.HasAttackPressed || inputReader.HasInteractPressed)
            {
                DialogueController.Instance?.Advance();
                inputReader.ConsumeAttackPressed();
                inputReader.ConsumeInteractPressed();
            }

            inputReader.TickBuffers();
            motor.StopHorizontalMovement();
            ChangeState(MovementState.Idle);
            return;
        }

        actionController.TickActions();
        inputReader.TickBuffers();
        UpdateMovementState();
    }

    void FixedUpdate()
    {
        if (conditionState == ConditionState.Dead)
            return;

        actionController.TickMovement(Time.fixedDeltaTime);
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
        groundSensor.Tick(deltaTime);
        isGrounded = groundSensor.IsGrounded;

        if (groundSensor.LandedThisFrame)
            jumpChance = MaxAirJumpCount;
    }

    public bool TryJump()
    {
        if (groundSensor.CanGroundJump)
        {
            StartJump(true);
            return true;
        }

        if (jumpChance > 0)
        {
            jumpChance--;
            StartJump(false);
            return true;
        }

        return false;
    }

    void StartJump(bool isGroundJump)
    {
        if (isGroundJump)
            jumpChance = MaxAirJumpCount;

        groundSensor.StartJump();
        isGrounded = false;
        motor.Jump();
    }

    public bool IsPushingIntoWall(float inputX)
    {
        return groundSensor.IsPushingIntoWall(inputX);
    }

    void SyncJumpHeldFromInputAction()
    {
        if (jumpAction == null)
            return;

        inputReader.SetJumpHeld(jumpAction.IsPressed());
    }

    public bool IsActionBlockedByCondition()
    {
        return conditionState == ConditionState.Controlled
            || conditionState == ConditionState.Stun
            || conditionState == ConditionState.Fear;
    }

    public bool IsMovementBlockedByCondition()
    {
        return IsActionBlockedByCondition()
            || conditionState == ConditionState.Root;
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
        bool pressed = inputValue.isPressed;
        inputReader.SetJump(pressed);

        if (!pressed && motor != null)
            motor.CutJump();
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
