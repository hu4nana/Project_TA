using System;

public enum MovementState
{
    Idle,
    Walk,
    Run,
    Dash,
    Jump
}

public enum ActionState
{
    None,
    Attack,
    Defence,
    Dodge,
    Parry,
    Skill
}

public enum ConditionState
{
    Normal,
    Invincible,
    Controlled,
    Dead,
    Stun,
    Root,
    Slow,
    Fear,
    Disarm
}

public static class EnumUtil<T> where T : struct, Enum
{
    public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    public static readonly int Count = Values.Length;

    public static T FromIndex(int index) => Values[index];
    public static int ToIndex(T value) => Array.IndexOf(Values, value);
}
