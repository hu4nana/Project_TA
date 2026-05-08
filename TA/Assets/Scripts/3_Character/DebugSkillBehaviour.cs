using UnityEngine;

public class DebugSkillBehaviour : SkillBehaviour
{
    public override void Cast(SkillContext context)
    {
        Debug.Log($"{context.caster.name} cast {SkillName} (freeCast: {context.freeCast})");
    }
}
