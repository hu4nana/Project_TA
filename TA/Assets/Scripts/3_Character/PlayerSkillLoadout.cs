using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillLoadout : MonoBehaviour
{
    [SerializeField] List<SkillBehaviour> equippedSkills = new();

    public IReadOnlyList<SkillBehaviour> EquippedSkills => equippedSkills;

    public SkillBehaviour GetSkill(int index)
    {
        if (index < 0 || index >= equippedSkills.Count)
            return null;

        return equippedSkills[index];
    }

    public void Clear()
    {
        equippedSkills.Clear();
    }

    public void EquipSkill(SkillBehaviour skill)
    {
        if (skill != null)
            equippedSkills.Add(skill);
    }
}
