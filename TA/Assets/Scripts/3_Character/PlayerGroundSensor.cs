using UnityEngine;

public sealed class PlayerGroundSensor
{
    readonly Player player;
    readonly float coyoteTime;
    readonly float groundIgnoreAfterJump;
    readonly float wallCheckDistance;
    readonly Vector2 wallCheckSize;

    float coyoteTimer;
    float groundIgnoreTimer;
    int wallDirection;

    public bool WasGrounded { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool LandedThisFrame { get; private set; }
    public bool CanGroundJump => IsGrounded || coyoteTimer > 0f;

    public PlayerGroundSensor(
        Player player,
        float coyoteTime,
        float groundIgnoreAfterJump,
        float wallCheckDistance,
        Vector2 wallCheckSize)
    {
        this.player = player;
        this.coyoteTime = coyoteTime;
        this.groundIgnoreAfterJump = groundIgnoreAfterJump;
        this.wallCheckDistance = wallCheckDistance;
        this.wallCheckSize = wallCheckSize;
    }

    public void Tick(float deltaTime)
    {
        UpdateGround(deltaTime);
        UpdateWall();
    }

    public void StartJump()
    {
        coyoteTimer = 0f;
        IsGrounded = false;
        groundIgnoreTimer = groundIgnoreAfterJump;
    }

    public bool IsPushingIntoWall(float inputX)
    {
        if (IsGrounded || wallDirection == 0 || Mathf.Abs(inputX) <= 0.01f)
            return false;

        return Mathf.Sign(inputX) == wallDirection;
    }

    void UpdateGround(float deltaTime)
    {
        bool rawGrounded = player.Grounded();

        if (groundIgnoreTimer > 0f)
        {
            groundIgnoreTimer -= deltaTime;
            if (player.Rigidbody.linearVelocity.y > 0.01f)
                rawGrounded = false;
        }

        WasGrounded = IsGrounded;
        IsGrounded = rawGrounded;
        LandedThisFrame = IsGrounded && !WasGrounded;

        if (IsGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= deltaTime;
    }

    void UpdateWall()
    {
        BoxCollider2D collider = player.Collider;
        if (collider == null)
        {
            wallDirection = 0;
            return;
        }

        Bounds bounds = collider.bounds;
        Vector2 origin = bounds.center;
        Vector2 leftOrigin = origin + Vector2.left * (bounds.extents.x + wallCheckDistance * 0.5f);
        Vector2 rightOrigin = origin + Vector2.right * (bounds.extents.x + wallCheckDistance * 0.5f);
        int mask = player.GroundMask;

        bool leftWall = Physics2D.OverlapBox(leftOrigin, wallCheckSize, 0f, mask) != null;
        bool rightWall = Physics2D.OverlapBox(rightOrigin, wallCheckSize, 0f, mask) != null;

        wallDirection = !IsGrounded && leftWall ? -1 : !IsGrounded && rightWall ? 1 : 0;
    }
}
