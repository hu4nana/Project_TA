using UnityEngine;

public enum AttackType
{
    Normal,
    Heavy,
    Projectile,
    Unblockable
}

[System.Serializable]
public struct AttackInfo
{
    public int damage;
    public Vector2 knockback;
    public AttackType attackType;
    public bool canBeParried;
    public bool canBeDodged;
    public bool opensCounterWindow;
    public GameObject source;
}

[System.Serializable]
public struct DamageInfo
{
    public int damage;
    public Vector2 knockback;
    public GameObject source;
}

public enum CounterTriggerType
{
    Parry,
    PerfectDodge
}
