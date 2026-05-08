using UnityEngine;

public class PlayerResourceController : MonoBehaviour
{
    [SerializeField] float maxATP = 100f;
    [SerializeField] float atpRegenPerSecond = 8f;
    [SerializeField] float attackHitATP = 4f;
    [SerializeField] float parryATP = 20f;
    [SerializeField] float perfectDodgeATP = 15f;

    public float CurrentATP { get; private set; }
    public float MaxATP => maxATP;

    public void Tick(float deltaTime)
    {
        AddATP(atpRegenPerSecond * deltaTime);
    }

    public void AddATP(float value)
    {
        CurrentATP = Mathf.Clamp(CurrentATP + value, 0f, maxATP);
    }

    public bool CanUse(float cost) => CurrentATP >= cost;

    public bool TryConsume(float cost, bool freeCast = false)
    {
        if (freeCast)
            return true;

        if (!CanUse(cost))
            return false;

        CurrentATP -= cost;
        return true;
    }

    public void OnAttackHit() => AddATP(attackHitATP);
    public void OnParrySuccess() => AddATP(parryATP);
    public void OnPerfectDodgeSuccess() => AddATP(perfectDodgeATP);
}
