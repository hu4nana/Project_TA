using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour, IDamageable
{
    [SerializeField] float stunDuration = 0.25f;

    Character character;
    PlayerDefense defense;
    PlayerResourceController resources;
    PlayerMotor motor;
    float stunTimer;

    public void Initialize(Character owner, PlayerDefense playerDefense, PlayerResourceController resourceController, PlayerMotor playerMotor)
    {
        character = owner;
        defense = playerDefense;
        resources = resourceController;
        motor = playerMotor;
    }

    public void Tick(float deltaTime)
    {
        if (character == null || character.conditionState != ConditionState.Stun)
            return;

        stunTimer -= deltaTime;
        if (stunTimer <= 0f)
            character.ChangeState(ConditionState.Normal);
    }

    public void TakeDamage(DamageInfo info)
    {
        character.LossHP(info.damage);
        motor.ApplyKnockback(info.knockback);

        if (character.HP <= 0)
        {
            character.ChangeState(ConditionState.Dead);
            return;
        }

        stunTimer = stunDuration;
        character.ChangeState(ActionState.None);
        character.ChangeState(ConditionState.Stun);
    }

    public bool ReceiveAttack(AttackInfo attack)
    {
        if (character.conditionState == ConditionState.Dead)
            return false;

        if (defense.TryDefend(character, attack, resources))
            return true;

        TakeDamage(new DamageInfo
        {
            damage = attack.damage,
            knockback = attack.knockback,
            source = attack.source
        });
        return true;
    }
}
