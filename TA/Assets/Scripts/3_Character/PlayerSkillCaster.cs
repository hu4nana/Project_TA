using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerSkillLoadout))]
public class PlayerSkillCaster : MonoBehaviour
{
    readonly Dictionary<int, float> cooldowns = new();
    readonly List<int> cooldownKeys = new();

    Player player;
    PlayerMotor motor;
    PlayerSkillLoadout loadout;
    PlayerFeedbacks feedbacks;
    float castTimer;

    public bool IsCasting => castTimer > 0f;
    public int SkillCount => loadout != null ? loadout.EquippedSkills.Count : 0;

    void Awake()
    {
        player = GetComponent<Player>();
        motor = GetComponent<PlayerMotor>();
        loadout = GetComponent<PlayerSkillLoadout>();
        feedbacks = GetComponent<PlayerFeedbacks>();
    }

    public void Tick(Character character, PlayerResourceController resources, float deltaTime)
    {
        if (castTimer > 0f)
        {
            castTimer -= deltaTime;
            if (castTimer <= 0f)
                character.ChangeState(ActionState.None);
        }

        TickCooldowns(deltaTime);
        HandleSelectionInput(character, resources);
    }

    void TickCooldowns(float deltaTime)
    {
        if (cooldowns.Count == 0)
            return;

        cooldownKeys.Clear();
        cooldownKeys.AddRange(cooldowns.Keys);

        for (int i = 0; i < cooldownKeys.Count; i++)
        {
            int key = cooldownKeys[i];
            float next = cooldowns[key] - deltaTime;
            if (next <= 0f)
                cooldowns.Remove(key);
            else
                cooldowns[key] = next;
        }
    }

    void HandleSelectionInput(Character character, PlayerResourceController resources)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame) TryCast(character, resources, 0);
        if (keyboard.digit2Key.wasPressedThisFrame) TryCast(character, resources, 1);
        if (keyboard.digit3Key.wasPressedThisFrame) TryCast(character, resources, 2);
    }

    public bool TryCast(Character character, PlayerResourceController resources, int index)
    {
        SkillBehaviour skillPrefab = loadout != null ? loadout.GetSkill(index) : null;
        if (skillPrefab == null || IsCasting || IsOnCooldown(index))
            return false;

        bool canFreeCast = CounterWindowSystem.Instance != null && CounterWindowSystem.Instance.FreeSkillAvailable;
        if (!canFreeCast && !resources.CanUse(skillPrefab.AtpCost))
            return false;

        SkillContext context = new SkillContext
        {
            caster = player,
            motor = motor,
            direction = motor != null && motor.FacingRight ? Vector2.right : Vector2.left,
            freeCast = canFreeCast && CounterWindowSystem.Instance.TryConsumeFreeSkill()
        };

        if (!skillPrefab.CanCast(context))
            return false;

        if (!resources.TryConsume(skillPrefab.AtpCost, context.freeCast))
            return false;

        castTimer = skillPrefab.CastDuration;
        cooldowns[index] = skillPrefab.Cooldown;
        character.ChangeState(ActionState.Skill);

        GameObject skillObject = Instantiate(skillPrefab.gameObject, transform.position, Quaternion.identity);
        SkillBehaviour skillInstance = skillObject.GetComponent<SkillBehaviour>();
        skillInstance.Cast(context);
        feedbacks?.PlaySkillCast();
        Destroy(skillObject, 1f);
        return true;
    }

    public float GetCooldownRemaining(int index) => cooldowns.TryGetValue(index, out float value) ? Mathf.Max(0f, value) : 0f;

    bool IsOnCooldown(int index) => cooldowns.TryGetValue(index, out float value) && value > 0f;
}

