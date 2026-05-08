using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] float attackDuration = 0.18f;
    [SerializeField] Vector2 attackOffset = new(0.9f, 0f);
    [SerializeField] Vector2 attackSize = new(1.2f, 1f);
    [SerializeField] int attackDamage = 1;
    [SerializeField] Vector2 attackKnockback = new(2.5f, 1f);
    [SerializeField] LayerMask hitMask = ~0;

    float attackTimer;
    bool hitApplied;
    Player player;
    PlayerMotor motor;
    PlayerResourceController resources;
    PlayerFeedbacks feedbacks;

    public bool IsAttacking => attackTimer > 0f;

    void Awake()
    {
        player = GetComponent<Player>();
        motor = GetComponent<PlayerMotor>();
        resources = GetComponent<PlayerResourceController>();
        feedbacks = GetComponent<PlayerFeedbacks>();
    }

    public bool TryStartAttack(Character character)
    {
        if (IsAttacking || character.conditionState != ConditionState.Normal)
            return false;

        attackTimer = attackDuration;
        hitApplied = false;
        character.ChangeState(ActionState.Attack);
        feedbacks?.PlayBasicAttack();
        return true;
    }

    public void Tick(Character character, float deltaTime)
    {
        if (attackTimer <= 0f)
            return;

        attackTimer -= deltaTime;

        if (!hitApplied && attackTimer <= attackDuration * 0.5f)
        {
            hitApplied = true;
            ApplyHit();
        }

        if (attackTimer <= 0f)
            character.ChangeState(ActionState.None);
    }

    void ApplyHit()
    {
        Vector2 dir = motor != null && motor.FacingRight ? Vector2.right : Vector2.left;
        Vector2 center = (Vector2)transform.position + new Vector2(attackOffset.x * dir.x, attackOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackSize, 0f, hitMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject == gameObject)
                continue;

            IDamageable damageable = hits[i].GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            damageable.TakeDamage(new DamageInfo
            {
                damage = attackDamage,
                knockback = new Vector2(attackKnockback.x * dir.x, attackKnockback.y),
                source = gameObject
            });
            resources?.OnAttackHit();
        }
    }
}
