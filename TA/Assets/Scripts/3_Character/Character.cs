using UnityEngine;

public class Character : MonoBehaviour
{
    public MovementState movementState;
    public ActionState actionState;
    public ConditionState conditionState;

    [Header("Stats")]
    public int MaxHP = 5;

    [Header("Movement")]
    public float walkSpeed;
    public float dashForce;

    [Header("Jump")]
    public int maxJumpChance;
    public int jumpChance;

    public int HP { get; private set; }
    public bool IsGrounded => isGrounded;
    public Rigidbody2D Rigidbody => rigid;
    public Vector2 GroundCheckCenter => groundCheckCenter;
    public Vector2 GroundCheckSize => groundCheckSize;
    public LayerMask GroundMask => groundMask;

    protected Rigidbody2D rigid;
    protected Animator ani;
    protected BoxCollider2D col;

    LayerMask groundMask;
    readonly float edgeInset = 0.02f;
    readonly float maxSlopeAngle = 60f;
    float minGroundNormalY;
    readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];

    [SerializeField] protected bool isGrounded;
    protected Vector2 groundCheckCenter;
    protected Vector2 groundCheckSize;

    public virtual void Initialize()
    {
        HP = MaxHP;
        jumpChance = maxJumpChance;
        movementState = MovementState.Idle;
        actionState = ActionState.None;
        conditionState = ConditionState.Normal;

        rigid = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();

        minGroundNormalY = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        groundMask = LayerMask.GetMask(
            "Platform",
            "Platforms",
            "OneWayPlatforms",
            "MovingPlatforms",
            "MovingOneWayPlatforms",
            "PlatformsPlayerOnly",
            "MidHeightOneWayPlatforms");
    }

    public void ChangeState(MovementState newState)
    {
        if (movementState == newState)
            return;

        movementState = newState;
        SetAnimatorBool(newState);
    }

    public void ChangeState(ActionState newState)
    {
        if (actionState == newState)
            return;

        actionState = newState;
        SetAnimatorBool(newState);
    }

    public void ChangeState(ConditionState newState)
    {
        if (conditionState == newState)
            return;

        conditionState = newState;
        SetAnimatorBool(newState);
    }

    public virtual void RegainHP(int value)
    {
        HP = Mathf.Min(MaxHP, HP + value);
    }

    public virtual void LossHP(int value)
    {
        HP = Mathf.Max(0, HP - value);
    }

    public bool Grounded()
    {
        if (col == null)
            return false;

        Bounds bounds = col.bounds;
        groundCheckCenter = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        groundCheckSize = new Vector2(Mathf.Max(0.05f, bounds.size.x - edgeInset * 2f), 0.1f);

        int contactCount = col.GetContacts(groundContacts);
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = groundContacts[i];
            if (((1 << contact.collider.gameObject.layer) & groundMask) != 0 && contact.normal.y >= minGroundNormalY)
                return true;
        }

        return Physics2D.OverlapBox(groundCheckCenter, groundCheckSize, 0f, groundMask) != null;
    }

    void SetAnimatorBool<T>(T state) where T : struct, System.Enum
    {
        if (ani == null)
            return;

        for (int i = 0; i < EnumUtil<T>.Count; i++)
        {
            T value = EnumUtil<T>.FromIndex(i);
            ani.SetBool(value.ToString(), value.Equals(state));
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col)
            col = GetComponent<BoxCollider2D>();
        if (!col)
            return;

        Bounds bounds = col.bounds;
        Vector3 left = new(bounds.min.x + edgeInset, bounds.min.y - 0.02f, 0f);
        Vector3 right = new(bounds.max.x - edgeInset, bounds.min.y - 0.02f, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, right);
    }
#endif
}
