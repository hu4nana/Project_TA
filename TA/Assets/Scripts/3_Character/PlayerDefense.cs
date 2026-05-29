using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    PlayerFeedbacks feedbacks;

    void Awake()
    {
        Player player = GetComponentInParent<Player>();
        Transform root = player != null ? player.transform : transform;
        feedbacks = root.GetComponentInChildren<PlayerFeedbacks>(true);
    }

    [Header("Parry")]
    [SerializeField] float parryWindow = 0.15f;
    [SerializeField] float parryRecovery = 0.1f;

    [Header("Dodge")]
    [SerializeField] float dodgeInvincibleTime = 0.18f;
    [SerializeField] float dodgeDashEndBeforeInvincibleEnds = 0.1f;
    [SerializeField] float perfectDodgeWindow = 0.12f;

    float parryTimer;
    float parryRecoveryTimer;
    float dodgeInvincibleTimer;
    float dodgeActionTimer;
    float perfectDodgeTimer;

    public bool IsParrying => parryTimer > 0f;
    public bool IsParryRecovering => parryRecoveryTimer > 0f;
    public bool IsInvincible => dodgeInvincibleTimer > 0f;
    public bool IsPerfectDodgeWindowActive => perfectDodgeTimer > 0f;
    public float DodgeDashDuration => Mathf.Max(0f, dodgeInvincibleTime - dodgeDashEndBeforeInvincibleEnds);

    public void Tick(Character character, float deltaTime)
    {
        parryTimer -= deltaTime;
        parryRecoveryTimer -= deltaTime;
        dodgeInvincibleTimer -= deltaTime;
        dodgeActionTimer -= deltaTime;
        perfectDodgeTimer -= deltaTime;

        if (character.actionState == ActionState.Parry && parryTimer <= 0f && parryRecoveryTimer <= 0f)
            character.ChangeState(ActionState.None);

        if (character.actionState == ActionState.Dodge && dodgeActionTimer <= 0f)
            character.ChangeState(ActionState.None);
    }

    public bool TryStartParry(Character character)
    {
        if (character.conditionState != ConditionState.Normal || IsParrying || IsParryRecovering)
            return false;

        parryTimer = parryWindow;
        parryRecoveryTimer = parryWindow + parryRecovery;
        character.ChangeState(ActionState.Parry);
        feedbacks?.PlayParryStart();
        return true;
    }

    public bool TryStartDodge(Character character)
    {
        if (character.conditionState != ConditionState.Normal || IsInvincible)
            return false;

        dodgeInvincibleTimer = dodgeInvincibleTime;
        dodgeActionTimer = DodgeDashDuration;
        perfectDodgeTimer = perfectDodgeWindow;
        character.ChangeState(ActionState.Dodge);
        feedbacks?.PlayDodgeStart();
        return true;
    }

    public bool TryDefend(Character character, AttackInfo attack, PlayerResourceController resourceController)
    {
        if (attack.canBeParried && IsParrying)
        {
            resourceController.OnParrySuccess();
            CounterWindowSystem.Instance?.Open(CounterTriggerType.Parry);
            character.ChangeState(ActionState.None);
            feedbacks?.PlayParrySuccess();
            return true;
        }

        if (attack.canBeDodged && perfectDodgeTimer > 0f)
        {
            resourceController.OnPerfectDodgeSuccess();
            CounterWindowSystem.Instance?.Open(CounterTriggerType.PerfectDodge);
            feedbacks?.PlayPerfectDodge();
            return true;
        }

        return IsInvincible;
    }
}
