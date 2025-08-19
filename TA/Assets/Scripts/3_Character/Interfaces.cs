using UnityEngine;

public interface ICharacterMovement
{
    void Idle();
    void Move(Vector2 moveVector);
    void Run();
    void Dash();
    void Jump();
}

public interface ICharacterAction
{
    void None();
    void Attack();
    void Defence();
    void Dodge();
    void Parry();
    void Skill();
}

public interface ICharacterCondition
{

}