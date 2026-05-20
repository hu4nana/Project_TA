using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HitboxAttackData
{
    public Vector2 offset;
    public Vector2 size;
    public int damage;
    public Vector2 knockback;
    public LayerMask hitMask;
}

public static class CombatHitbox
{
    static readonly HashSet<IDamageable> damagedTargets = new();

    static float debugTimer;

    public static Vector2 DebugHitboxCenter { get; private set; }
    public static Vector2 DebugHitboxSize { get; private set; }
    public static string DebugLabel { get; private set; }
    public static Color DebugColor { get; private set; } = Color.white;
    public static bool HasDebugHitbox => debugTimer > 0f;

    public static void TickDebug(float deltaTime)
    {
        if (debugTimer > 0f)
            debugTimer -= deltaTime;
    }

    public static int ApplyBox(
        GameObject source,
        Vector2 origin,
        Vector2 direction,
        HitboxAttackData attack,
        string debugLabel,
        Color debugColor,
        Action<IDamageable> onHit = null)
    {
        Vector2 normalizedDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        Vector2 center = origin + new Vector2(attack.offset.x * normalizedDirection.x, attack.offset.y);

        ShowDebug(center, attack.size, debugLabel, debugColor);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attack.size, 0f, attack.hitMask);
        damagedTargets.Clear();

        int hitCount = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].GetComponentInParent<IDamageable>();
            if (damageable == null || damagedTargets.Contains(damageable))
                continue;

            if (damageable is Component component && component.gameObject == source)
                continue;

            damagedTargets.Add(damageable);
            damageable.TakeDamage(new DamageInfo
            {
                damage = attack.damage,
                knockback = new Vector2(attack.knockback.x * normalizedDirection.x, attack.knockback.y),
                source = source
            });

            onHit?.Invoke(damageable);
            hitCount++;
        }

        return hitCount;
    }

    static void ShowDebug(Vector2 center, Vector2 size, string label, Color color)
    {
        DebugHitboxCenter = center;
        DebugHitboxSize = size;
        DebugLabel = label;
        DebugColor = color;
        debugTimer = 0.2f;
    }
}
