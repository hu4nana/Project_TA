using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] float airControlMultiplier = 0.75f;
    [SerializeField] float fallGravityMultiplier = 1.8f;
    [SerializeField] float lowJumpGravityMultiplier = 2.2f;

    Character character;
    Rigidbody2D rigid;

    float dodgeTimer;
    Vector2 dodgeVelocity;

    public bool IsDodging => dodgeTimer > 0f;
    public bool FacingRight { get; private set; } = true;

    public void Initialize(Character owner)
    {
        character = owner;
        rigid = owner.GetComponent<Rigidbody2D>();
        rigid.freezeRotation = true;
    }

    public void Tick(float deltaTime)
    {
        if (dodgeTimer > 0f)
        {
            dodgeTimer -= deltaTime;
            rigid.linearVelocity = new Vector2(dodgeVelocity.x, rigid.linearVelocity.y);
        }

        ApplyBetterGravity();
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

    public void Jump()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(Vector2.up * character.jumpForce, ForceMode2D.Impulse);
    }

    public void CutJump()
    {
        if (rigid.linearVelocity.y > 0f)
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y * 0.5f);
    }

    public void StartDodge(float inputX)
    {
        float direction = Mathf.Abs(inputX) > 0.01f ? Mathf.Sign(inputX) : (FacingRight ? 1f : -1f);
        dodgeVelocity = new Vector2(direction * character.dashForce, 0f);
        dodgeTimer = character.dashTime;
        SetFacing(direction > 0f);
    }

    public void ApplyKnockback(Vector2 force)
    {
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(force, ForceMode2D.Impulse);
    }

    void ApplyBetterGravity()
    {
        if (character == null || character.IsGrounded)
            return;

        float multiplier = rigid.linearVelocity.y < 0f ? fallGravityMultiplier : lowJumpGravityMultiplier;
        rigid.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplier - 1f) * Time.fixedDeltaTime;
    }

    void SetFacing(bool facingRight)
    {
        if (FacingRight == facingRight)
            return;

        FacingRight = facingRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }
}
