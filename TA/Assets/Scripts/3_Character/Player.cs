using UnityEngine;
using UnityEngine.InputSystem;


public class Player : Character
{
    #region InputVariable

    Vector2 inputMove;
    bool inputJump;
    bool inputAttack;
    bool inputDash;
    bool inputTurnCatch;
    bool inputParry;

    #endregion


    private void Start()
    {
        Initialize();
    }

    #region InputActions
    public void OnMove(InputValue inputValue)
    {
        inputMove = Vector2.right * inputValue.Get<Vector2>();
    }
    public void OnJump(InputValue inputValue)
    {
    }
    public void OnAttack(InputValue inputValue)
    {
        inputAttack = inputValue.isPressed;
    }
    public void OnDash(InputValue inputValue)
    {
        inputDash = inputValue.isPressed;
    }
    public void OnTurnCatch(InputValue inputValue)
    {
        inputTurnCatch = inputValue.isPressed;
    }
    public void OnParry(InputValue inputValue)
    {
        inputParry = inputValue.isPressed;
    }
    #endregion

    private void FixedUpdate()
    {
    }



    void TurnCatch()
    {
        if (inputTurnCatch)
        {

        }
    }

    public override void Initialize()
    {
        base.Initialize();
    }
}
