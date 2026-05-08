using UnityEngine;

public class MeleeSkillBehaviour : SkillBehaviour
{
    [SerializeField] Vector2 hitboxOffset = new(1f, 0f);
    [SerializeField] Vector2 hitboxSize = new(1.5f, 1f);
    [SerializeField] int damage = 1;
    [SerializeField] Vector2 knockback = new(3f, 1.5f);
    [SerializeField] LayerMask hitMask = ~0;

    public static Vector2 DebugHitboxCenter { get; private set; }
    public static Vector2 DebugHitboxSize { get; private set; }
    public static bool HasDebugHitbox => debugTimer > 0f;

    static float debugTimer;

    void Update()
    {
        if (debugTimer > 0f)
            debugTimer -= Time.unscaledDeltaTime;
    }

    public override void Cast(SkillContext context)
    {
        Vector2 direction = context.direction.sqrMagnitude > 0f ? context.direction.normalized : Vector2.right;
        Vector2 center = (Vector2)context.caster.transform.position + new Vector2(hitboxOffset.x * direction.x, hitboxOffset.y);
        DebugHitboxCenter = center;
        DebugHitboxSize = hitboxSize;
        debugTimer = 0.2f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitboxSize, 0f, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].GetComponent<IDamageable>();
            if (damageable == null || hits[i].gameObject == context.caster.gameObject)
                continue;

            damageable.TakeDamage(new DamageInfo
            {
                damage = damage,
                knockback = new Vector2(knockback.x * direction.x, knockback.y),
                source = context.caster.gameObject
            });
        }

        Debug.Log($"{context.caster.name} cast {SkillName} (freeCast: {context.freeCast})");
    }
}
