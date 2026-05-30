using UnityEngine;

public class EnemyAttackEmitter : MonoBehaviour
{
    [SerializeField] AttackInfo attackInfo = new AttackInfo
    {
        damage = 1,
        knockback = new Vector2(3f, 2f),
        attackType = AttackType.Normal,
        canBeParried = true,
        canBeDodged = true,
        opensCounterWindow = true,
    };
    [SerializeField] float repeatDelay = 0.35f;

    float cooldownTimer;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (cooldownTimer > 0f)
            return;

        Player player = other.GetComponentInParent<Player>();
        PlayerHitReceiver receiver = player != null
            ? player.GetComponentInChildren<PlayerHitReceiver>(true)
            : other.GetComponentInParent<PlayerHitReceiver>();

        if (!receiver)
            return;

        AttackInfo resolvedAttack = attackInfo;
        resolvedAttack.source = gameObject;
        resolvedAttack.knockback = ResolveKnockbackDirection(receiver.transform.position);
        receiver.ReceiveAttack(resolvedAttack);
        cooldownTimer = repeatDelay;
    }

    Vector2 ResolveKnockbackDirection(Vector3 targetPosition)
    {
        float direction = Mathf.Sign(targetPosition.x - transform.position.x);
        if (Mathf.Approximately(direction, 0f))
            direction = transform.localScale.x >= 0f ? 1f : -1f;

        return new Vector2(Mathf.Abs(attackInfo.knockback.x) * direction, attackInfo.knockback.y);
    }
}
