using UnityEngine;

public enum PlayerJumpType
{
    Ground,
    Air
}

public class PlayerMotor : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] float airControlMultiplier = 0.75f;

    [Header("Jump Feel")]
    [SerializeField] float jumpHoldDuration = 0.2f;
    [SerializeField] float heldJumpMinVelocity = 4.5f;

    [Header("Gravity")]
    [SerializeField] float fallGravityMultiplier = 2.4f;
    [SerializeField] float lowJumpGravityMultiplier = 3.2f;
    [SerializeField] float hangGravityMultiplier = 0.55f;
    [SerializeField] float hangVelocityThreshold = 1.2f;
    [SerializeField] float maxFallSpeed = 18f;

    Character character;
    Rigidbody2D rigid;

    float dodgeTimer;
    float jumpHoldTimer;
    float defaultGravityScale;
    bool jumpActive;
    bool jumpCutApplied;
    Vector2 dodgeVelocity;

    public bool IsDodging => dodgeTimer > 0f;
    public bool FacingRight { get; private set; } = true;
    public PlayerJumpType LastJumpType { get; private set; }

    public void Initialize(Character owner)
    {
        character = owner;
        rigid = owner.GetComponent<Rigidbody2D>();
        rigid.freezeRotation = true;
        defaultGravityScale = rigid.gravityScale;
    }

    public void Tick(float deltaTime, bool jumpHeld)
    {
        if (dodgeTimer > 0f)
        {
            dodgeTimer -= deltaTime;
            rigid.linearVelocity = new Vector2(dodgeVelocity.x, 0f);

            if (dodgeTimer <= 0f)
                EndDodge();

            return;
        }

        if (!Mathf.Approximately(rigid.gravityScale, defaultGravityScale))
            rigid.gravityScale = defaultGravityScale;

        if (jumpActive && !jumpHeld && !jumpCutApplied && rigid.linearVelocity.y > 0f)
            CutJump();

        ApplyHeldJump(jumpHeld, deltaTime);
        ApplyGravity(jumpHeld, deltaTime);
        ClampFallSpeed();

        if (rigid.linearVelocity.y <= 0f && character != null && character.IsGrounded)
            jumpActive = false;
    }

    public void Move(float inputX)
    {
        if (IsDodging)
            return;

        float control = character.IsGrounded ? 1f : airControlMultiplier;
        rigid.linearVelocity = new Vector2(inputX * character.walkSpeed * control, rigid.linearVelocity.y);

        if (Mathf.Abs(inputX) > 0.01f)
            SetFacing(inputX > 0f);
    }

    public void Jump(PlayerJumpType jumpType)
    {
        LastJumpType = jumpType;
        jumpActive = true;
        jumpCutApplied = false;
        jumpHoldTimer = jumpHoldDuration;
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, character.jumpForce);
    }

    public void CutJump()
    {
        if (jumpCutApplied)
            return;

        jumpCutApplied = true;
        jumpHoldTimer = 0f;

        if (rigid.linearVelocity.y > 0f)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
    }

    public void StartDodge(float duration)
    {
        float direction = FacingRight ? 1f : -1f;
        dodgeVelocity = new Vector2(direction * character.dashForce, 0f);
        dodgeTimer = duration;
        rigid.gravityScale = 0f;
        rigid.linearVelocity = new Vector2(dodgeVelocity.x, 0f);
    }

    void EndDodge()
    {
        dodgeTimer = 0f;
        rigid.gravityScale = defaultGravityScale;
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
    }

    public void StopHorizontalMovement()
    {
        if (IsDodging)
            return;

        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }

    public void ApplyKnockback(Vector2 force)
    {
        jumpActive = false;
        jumpHoldTimer = 0f;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(force, ForceMode2D.Impulse);
    }

    void ApplyHeldJump(bool jumpHeld, float deltaTime)
    {
        if (!jumpActive || !jumpHeld || jumpCutApplied || jumpHoldTimer <= 0f || rigid.linearVelocity.y <= 0f)
            return;

        jumpHoldTimer -= deltaTime;
        if (rigid.linearVelocity.y < heldJumpMinVelocity)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, heldJumpMinVelocity);
    }

    void ApplyGravity(bool jumpHeld, float deltaTime)
    {
        if (character == null || character.IsGrounded)
            return;

        float multiplier;
        if (jumpActive && jumpHeld && !jumpCutApplied && Mathf.Abs(rigid.linearVelocity.y) <= hangVelocityThreshold)
            multiplier = hangGravityMultiplier;
        else if (rigid.linearVelocity.y < 0f)
            multiplier = fallGravityMultiplier;
        else
            multiplier = jumpHeld && !jumpCutApplied ? 1f : lowJumpGravityMultiplier;

        rigid.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1f) * deltaTime;
    }

    void ClampFallSpeed()
    {
        if (rigid.linearVelocity.y < -maxFallSpeed)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, -maxFallSpeed);
    }

    void SetFacing(bool facingRight)
    {
        if (FacingRight == facingRight)
            return;

        FacingRight = facingRight;
        Transform target = character != null ? character.transform : transform;
        Vector3 scale = target.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        target.localScale = scale;
    }
}
