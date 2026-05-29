using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Horizontal")]
    [Tooltip("공중에서 좌우 이동이 얼마나 먹는지. 1이면 지상과 같고, 낮을수록 공중 제어가 약해집니다.")]
    [SerializeField] float airControlMultiplier = 0.75f;

    [Header("Jump")]
    [Tooltip("점프 버튼을 눌렀을 때 위로 튀어오르는 힘. 점프 높이를 가장 직접적으로 조절합니다.")]
    [SerializeField] float jumpForce = 10f;

    [Header("Gravity")]
    [Tooltip("떨어질 때 추가 중력 배율. 높을수록 더 빨리 떨어집니다.")]
    [SerializeField] float fallGravityMultiplier = 1.8f;
    [Tooltip("상승 중 추가 중력 배율. 높을수록 점프가 낮고 타이트해집니다.")]
    [SerializeField] float lowJumpGravityMultiplier = 2.2f;

    Character character;
    Rigidbody2D rigid;

    float dodgeTimer;
    float defaultGravityScale;
    Vector2 dodgeVelocity;

    public bool IsDodging => dodgeTimer > 0f;
    public bool FacingRight { get; private set; } = true;

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

    public void Jump()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
