using UnityEngine;

public abstract class SkillBehaviour : MonoBehaviour
{
    [SerializeField] string skillName = "Skill";
    [SerializeField] float atpCost = 25f;
    [SerializeField] float cooldown = 1f;
    [SerializeField] float castDuration = 0.2f;

    public string SkillName => skillName;
    public float AtpCost => atpCost;
    public float Cooldown => cooldown;
    public float CastDuration => castDuration;

    public virtual bool CanCast(SkillContext context) => true;
    public abstract void Cast(SkillContext context);
}

public struct SkillContext
{
    public Player caster;
    public PlayerMotor motor;
    public Vector2 direction;
    public bool freeCast;
}
