using UnityEngine;

public class PlayerHurtboxController : MonoBehaviour
{
    [Header("Model-Favoring Hurtbox")]
    [SerializeField] Vector2 standingSizeMultiplier = new(0.82f, 0.9f);
    [SerializeField] Vector2 standingOffset = Vector2.zero;

    [Header("Run Hurtbox")]
    [SerializeField] Vector2 runningSizeMultiplier = new(0.9f, 0.85f);
    [SerializeField] float runVelocityThreshold = 0.1f;

    Player player;
    BoxCollider2D bodyCollider;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigid;
    Vector2 standingSize;
    Vector2 runningSize;

    void Awake()
    {
        player = GetComponentInParent<Player>();
        if (player == null)
            return;

        bodyCollider = player.GetComponent<BoxCollider2D>();
        rigid = player.GetComponent<Rigidbody2D>();
        spriteRenderer = player.GetComponentInChildren<SpriteRenderer>(true);

        if (bodyCollider == null)
            return;

        Vector2 sourceSize = bodyCollider.size;
        if (spriteRenderer != null)
            sourceSize = spriteRenderer.sprite.bounds.size;

        standingSize = new Vector2(sourceSize.x * standingSizeMultiplier.x, sourceSize.y * standingSizeMultiplier.y);
        runningSize = new Vector2(standingSize.x * runningSizeMultiplier.x, standingSize.y * runningSizeMultiplier.y);

        Apply(standingSize);
    }

    void LateUpdate()
    {
        if (bodyCollider == null || rigid == null || player == null)
            return;

        bool running = player.IsGrounded && Mathf.Abs(rigid.linearVelocity.x) > runVelocityThreshold;
        Apply(running ? runningSize : standingSize);
    }

    void Apply(Vector2 size)
    {
        bodyCollider.size = size;
        bodyCollider.offset = standingOffset;
    }
}
