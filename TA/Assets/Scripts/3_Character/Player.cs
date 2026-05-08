using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerDefense))]
[RequireComponent(typeof(PlayerResourceController))]
[RequireComponent(typeof(PlayerHitReceiver))]
[RequireComponent(typeof(PlayerSkillCaster))]
[RequireComponent(typeof(PlayerSkillLoadout))]
public class Player : Character
{
    [Header("Jump Assist")]
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float groundIgnoreAfterJump = 0.12f;

    PlayerInputReader inputReader;
    PlayerMotor motor;
    PlayerCombat combat;
    PlayerDefense defense;
    PlayerResourceController resources;
    PlayerHitReceiver hitReceiver;
    PlayerSkillCaster skillCaster;

    bool wasGrounded;
    float coyoteTimer;
    float groundIgnoreTimer;

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
        resources = GetOrAdd<PlayerResourceController>();
        hitReceiver = GetOrAdd<PlayerHitReceiver>();
        GetOrAdd<PlayerSkillLoadout>();
        skillCaster = GetOrAdd<PlayerSkillCaster>();

        motor.Initialize(this);
        hitReceiver.Initialize(this, defense, resources, motor);
    }

    void Update()
    {
        UpdateGroundState(Time.deltaTime);
        resources.Tick(Time.deltaTime);
        combat.Tick(this, Time.deltaTime);
        defense.Tick(this, Time.deltaTime);
        hitReceiver.Tick(Time.deltaTime);
        skillCaster.Tick(this, resources, Time.unscaledDeltaTime);

        if (conditionState == ConditionState.Dead)
            return;

        if (!inputReader.JumpHeld)
            motor.CutJump();

        HandleActions();
        UpdateMovementState();
    }

    void FixedUpdate()
    {
        if (conditionState == ConditionState.Dead)
            return;

        float moveX = inputReader.Move.x;
        if (actionState != ActionState.Parry)
            motor.Move(moveX);

        motor.Tick(Time.fixedDeltaTime);
    }

    void HandleActions()
    {
        if (inputReader.ConsumeParryPressed())
            defense.TryStartParry(this);

        if (inputReader.ConsumeDashPressed() && defense.TryStartDodge(this))
            motor.StartDodge(inputReader.Move.x);

        if (inputReader.ConsumeJumpPressed())
            TryJump();

        if (inputReader.ConsumeAttackPressed() && !skillCaster.IsCasting)
            combat.TryStartAttack(this);
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
        bool rawGrounded = Grounded(out groundHIt);

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

    public void OnTurnCatch(InputValue inputValue)
    {
    }

    public void OnParry(InputValue inputValue)
    {
        inputReader.SetParry(inputValue.isPressed);
    }
    #endregion
}
