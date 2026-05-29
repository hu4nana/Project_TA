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

    [Header("Gravity")]
    [SerializeField] float fallGravityMultiplier = 1.8f;
    [SerializeField] float lowJumpGravityMultiplier = 2.2f;

    Character character;
    Rigidbody2D rigid;

    float dodgeTimer;
    float defaultGravityScale;
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

        if (!jumpHeld)
            CutJump();

        ApplyBetterGravity(deltaTime);
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
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(Vector2.up * character.jumpForce, ForceMode2D.Impulse);
    }

    public void CutJump()
    {
        if (rigid.linearVelocity.y > 0f)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y * 0.5f);
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
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(force, ForceMode2D.Impulse);
    }

    void ApplyBetterGravity(float deltaTime)
    {
        if (character == null || character.IsGrounded)
            return;

        float multiplier = rigid.linearVelocity.y < 0f ? fallGravityMultiplier : lowJumpGravityMultiplier;
        rigid.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1f) * deltaTime;
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
