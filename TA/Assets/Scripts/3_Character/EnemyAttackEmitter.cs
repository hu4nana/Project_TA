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

        PlayerHitReceiver receiver = other.GetComponent<PlayerHitReceiver>();
        if (!receiver)
            return;

        attackInfo.source = gameObject;
        receiver.ReceiveAttack(attackInfo);
        cooldownTimer = repeatDelay;
    }
}
