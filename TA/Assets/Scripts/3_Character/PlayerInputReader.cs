using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool JumpHeld { get; private set; }

    bool jumpPressed;
    bool attackPressed;
    bool dashPressed;
    bool parryPressed;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        bool keyboardJumpPressed = keyboard != null && keyboard.zKey.wasPressedThisFrame;
        bool gamepadJumpPressed = gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        bool keyboardHeld = keyboard != null && keyboard.zKey.isPressed;
        bool gamepadHeld = gamepad != null && gamepad.buttonSouth.isPressed;

        if (keyboardJumpPressed || gamepadJumpPressed)
            jumpPressed = true;

        JumpHeld = keyboardHeld || gamepadHeld;
    }

    public void SetMove(Vector2 value) => Move = value;

    public void SetJump(bool pressed)
    {
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
}
