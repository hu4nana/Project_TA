using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] float attackDuration = 0.18f;
    [SerializeField] HitboxAttackData basicAttack = new HitboxAttackData
    {
        offset = new Vector2(1.05f, 0f),
        size = new Vector2(1.8f, 1.4f),
        damage = 1,
        knockback = new Vector2(2.5f, 1f),
        hitMask = ~0
    };

    float attackTimer;
    bool hitApplied;
    Player player;
    PlayerMotor motor;
    PlayerResourceController resources;
    PlayerFeedbacks feedbacks;

    public bool IsAttacking => attackTimer > 0f;

    void Awake()
    {
        player = GetComponentInParent<Player>();
        Transform root = player != null ? player.transform : transform;
        motor = root.GetComponentInChildren<PlayerMotor>(true);
        resources = root.GetComponentInChildren<PlayerResourceController>(true);
        feedbacks = root.GetComponentInChildren<PlayerFeedbacks>(true);
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
        CombatHitbox.TickDebug(Time.unscaledDeltaTime);

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
        CombatHitbox.ApplyBox(
            player != null ? player.gameObject : gameObject,
            player != null ? player.transform.position : transform.position,
            dir,
            basicAttack,
            "Basic Attack Hitbox",
            Color.orange,
            _ => resources?.OnAttackHit());
    }
}
