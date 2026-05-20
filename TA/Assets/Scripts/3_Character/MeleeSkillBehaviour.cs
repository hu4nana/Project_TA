using UnityEngine;

public class MeleeSkillBehaviour : SkillBehaviour
{
    [SerializeField] HitboxAttackData attack = new HitboxAttackData
    {
        offset = new Vector2(1f, 0f),
        size = new Vector2(1.5f, 1f),
        damage = 1,
        knockback = new Vector2(3f, 1.5f),
        hitMask = ~0
    };

    public override void Cast(SkillContext context)
    {
        CombatHitbox.ApplyBox(
            context.caster.gameObject,
            context.caster.transform.position,
            context.direction,
            attack,
            $"Skill Hitbox: {SkillName}",
            Color.yellow);

        Debug.Log($"{context.caster.name} cast {SkillName} (freeCast: {context.freeCast})");
    }
}
