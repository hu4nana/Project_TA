using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyPatrolMotor : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] int startDirection = 1;
    [SerializeField] LayerMask groundMask;

    [Header("Checks")]
    [SerializeField] float wallCheckDistance = 0.12f;
    [SerializeField] Vector2 wallCheckSize = new(0.12f, 1.2f);
    [SerializeField] float ledgeCheckForwardOffset = 0.45f;
    [SerializeField] float ledgeCheckDownDistance = 0.35f;

    Rigidbody2D rigid;
    Collider2D bodyCollider;
    int direction;

    void Reset()
    {
        groundMask = LayerMask.GetMask(
            "Platform",
            "Platforms",
            "OneWayPlatforms",
            "MovingPlatforms",
            "MovingOneWayPlatforms",
            "PlatformsPlayerOnly",
            "MidHeightOneWayPlatforms");
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        rigid.freezeRotation = true;

        if (groundMask == 0)
            Reset();

        direction = startDirection >= 0 ? 1 : -1;
        ApplyFacing();
    }

    void FixedUpdate()
    {
        if (ShouldTurnAround())
            TurnAround();

        rigid.linearVelocity = new Vector2(direction * moveSpeed, rigid.linearVelocity.y);
    }

    bool ShouldTurnAround()
    {
        return IsWallAhead() || !HasGroundAhead();
    }

    bool IsWallAhead()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 checkCenter = new(
            bounds.center.x + direction * (bounds.extents.x + wallCheckDistance * 0.5f),
            bounds.center.y);

        return Physics2D.OverlapBox(checkCenter, wallCheckSize, 0f, groundMask) != null;
    }

    bool HasGroundAhead()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new(
            bounds.center.x + direction * ledgeCheckForwardOffset,
            bounds.min.y + 0.02f);

        return Physics2D.Raycast(origin, Vector2.down, ledgeCheckDownDistance, groundMask).collider != null;
    }

    void TurnAround()
    {
        direction *= -1;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Collider2D col = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
        if (col == null)
            return;

        int drawDirection = Application.isPlaying ? direction : (startDirection >= 0 ? 1 : -1);
        Bounds bounds = col.bounds;

        Vector2 wallCenter = new(
            bounds.center.x + drawDirection * (bounds.extents.x + wallCheckDistance * 0.5f),
            bounds.center.y);
        Vector2 ledgeOrigin = new(
            bounds.center.x + drawDirection * ledgeCheckForwardOffset,
            bounds.min.y + 0.02f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(wallCenter, wallCheckSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeCheckDownDistance);
    }
#endif
}
