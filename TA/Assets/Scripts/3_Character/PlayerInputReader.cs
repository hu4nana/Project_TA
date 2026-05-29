using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    const int BufferFrames = 6;

    public Vector2 Move { get; private set; }
    public bool JumpHeld { get; private set; }

    int jumpBuffer;
    int jumpReleaseBuffer;
    int attackBuffer;
    int dashBuffer;
    int parryBuffer;
    int interactBuffer;
    int skillBuffer;
    int skillPressedIndex = -1;

    public bool HasJumpPressed => jumpBuffer > 0;
    public bool HasAttackPressed => attackBuffer > 0;
    public bool HasDashPressed => dashBuffer > 0;
    public bool HasParryPressed => parryBuffer > 0;
    public bool HasInteractPressed => interactBuffer > 0;
    public bool HasSkillPressed => skillBuffer > 0 && skillPressedIndex >= 0;

    public void TickBuffers()
    {
        Tick(ref jumpBuffer);
        Tick(ref jumpReleaseBuffer);
        Tick(ref attackBuffer);
        Tick(ref dashBuffer);
        Tick(ref parryBuffer);
        Tick(ref interactBuffer);
        Tick(ref skillBuffer);

        if (skillBuffer <= 0)
            skillPressedIndex = -1;
    }

    public void SetMove(Vector2 value) => Move = value;

    public void SetJump(bool pressed)
    {
        if (JumpHeld && !pressed)
            jumpReleaseBuffer = BufferFrames;

        JumpHeld = pressed;
        if (pressed)
            jumpBuffer = BufferFrames;
    }

    public void SetAttack(bool pressed)
    {
        if (pressed)
            attackBuffer = BufferFrames;
    }

    public void SetDash(bool pressed)
    {
        if (pressed)
            dashBuffer = BufferFrames;
    }

    public void SetParry(bool pressed)
    {
        if (pressed)
            parryBuffer = BufferFrames;
    }

    public void SetInteract(bool pressed)
    {
        if (pressed)
            interactBuffer = BufferFrames;
    }

    public void SetSkill(int index, bool pressed)
    {
        if (!pressed)
            return;

        skillPressedIndex = index;
        skillBuffer = BufferFrames;
    }

    public void ConsumeJumpPressed() => jumpBuffer = 0;
    public void ConsumeAttackPressed() => attackBuffer = 0;
    public void ConsumeDashPressed() => dashBuffer = 0;
    public void ConsumeParryPressed() => parryBuffer = 0;
    public void ConsumeInteractPressed() => interactBuffer = 0;

    public bool ConsumeJumpReleased()
    {
        bool value = jumpReleaseBuffer > 0;
        jumpReleaseBuffer = 0;
        return value;
    }

    public bool TryGetSkillPressed(out int index)
    {
        index = skillPressedIndex;
        return HasSkillPressed;
    }

    public void ConsumeSkillPressed()
    {
        skillBuffer = 0;
        skillPressedIndex = -1;
    }

    public void ClearTransientInputs()
    {
        jumpBuffer = 0;
        jumpReleaseBuffer = 0;
        attackBuffer = 0;
        dashBuffer = 0;
        parryBuffer = 0;
        interactBuffer = 0;
        skillBuffer = 0;
        skillPressedIndex = -1;
    }

    void Tick(ref int buffer)
    {
        if (buffer > 0)
            buffer--;
    }
}
