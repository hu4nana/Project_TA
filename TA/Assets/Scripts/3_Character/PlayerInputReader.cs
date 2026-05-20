using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool JumpHeld { get; private set; }

    bool jumpPressed;
    bool attackPressed;
    bool dashPressed;
    bool parryPressed;
    bool interactPressed;
    int skillPressedIndex = -1;

    public void SetMove(Vector2 value) => Move = value;

    public void SetJump(bool pressed)
    {
        JumpHeld = pressed;
        if (pressed)
            jumpPressed = true;
    }

    public void SetAttack(bool pressed)
    {
        if (pressed)
            attackPressed = true;
    }

    public void SetDash(bool pressed)
    {
        if (pressed)
            dashPressed = true;
    }

    public void SetParry(bool pressed)
    {
        if (pressed)
            parryPressed = true;
    }

    public void SetInteract(bool pressed)
    {
        if (pressed)
            interactPressed = true;
    }

    public void SetSkill(int index, bool pressed)
    {
        if (pressed)
            skillPressedIndex = index;
    }

    public bool ConsumeJumpPressed()
    {
        bool value = jumpPressed;
        jumpPressed = false;
        return value;
    }

    public bool ConsumeAttackPressed()
    {
        bool value = attackPressed;
        attackPressed = false;
        return value;
    }

    public bool ConsumeDashPressed()
    {
        bool value = dashPressed;
        dashPressed = false;
        return value;
    }

    public bool ConsumeParryPressed()
    {
        bool value = parryPressed;
        parryPressed = false;
        return value;
    }

    public bool ConsumeInteractPressed()
    {
        bool value = interactPressed;
        interactPressed = false;
        return value;
    }

    public bool ConsumeSkillPressed(out int index)
    {
        index = skillPressedIndex;
        skillPressedIndex = -1;
        return index >= 0;
    }

    public void ClearTransientInputs()
    {
        jumpPressed = false;
        attackPressed = false;
        dashPressed = false;
        parryPressed = false;
        interactPressed = false;
        skillPressedIndex = -1;
    }
}
