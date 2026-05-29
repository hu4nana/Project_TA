using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    const int BufferFrames = 6;

    public Vector2 Move { get; private set; }
    public bool JumpHeld { get; private set; }

    bool eventJumpHeld;
    int jumpBuffer;
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
        Tick(ref attackBuffer);
        Tick(ref dashBuffer);
        Tick(ref parryBuffer);
        Tick(ref interactBuffer);
        Tick(ref skillBuffer);

        if (skillBuffer <= 0)
            skillPressedIndex = -1;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        bool keyboardHeld = keyboard != null && keyboard.zKey.isPressed;
        bool gamepadHeld = gamepad != null && gamepad.buttonSouth.isPressed;
        bool keyboardPressed = keyboard != null && keyboard.zKey.wasPressedThisFrame;
        bool gamepadPressed = gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;

        if (keyboardPressed || gamepadPressed)
            jumpBuffer = BufferFrames;

        SetJumpHeldState(eventJumpHeld || keyboardHeld || gamepadHeld);
    }

    public void SetMove(Vector2 value) => Move = value;

    public void SetJump(bool pressed)
    {
        eventJumpHeld = pressed;
        if (pressed)
            jumpBuffer = BufferFrames;

        SetJumpHeldState(pressed);
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
        attackBuffer = 0;
        dashBuffer = 0;
        parryBuffer = 0;
        interactBuffer = 0;
        skillBuffer = 0;
        skillPressedIndex = -1;
    }

    void SetJumpHeldState(bool held)
    {
        JumpHeld = held;
    }

    void Tick(ref int buffer)
    {
        if (buffer > 0)
            buffer--;
    }
}
