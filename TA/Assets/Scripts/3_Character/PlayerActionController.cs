public sealed class PlayerActionController
{
    readonly PlayerContext context;
    readonly PlayerAction[] actions;

    public PlayerActionController(PlayerContext context)
    {
        this.context = context;
        actions = new PlayerAction[]
        {
            new ParryPlayerAction(),
            new DodgePlayerAction(),
            new JumpPlayerAction(),
            new AttackPlayerAction(),
            new SkillPlayerAction(),
            new InteractPlayerAction()
        };
    }

    public void TickActions()
    {
        for (int i = 0; i < actions.Length; i++)
            actions[i].TryRun(context);
    }

    public void TickMovement(float deltaTime)
    {
        Player player = context.Player;
        PlayerMotor motor = context.Motor;
        PlayerInputReader input = context.Input;

        if (player.IsInputLocked)
        {
            motor.StopHorizontalMovement();
            motor.Tick(deltaTime, input.JumpHeld);
            return;
        }

        if (player.IsMovementBlockedByCondition())
        {
            if (player.conditionState == ConditionState.Root)
                motor.StopHorizontalMovement();

            motor.Tick(deltaTime, input.JumpHeld);
            return;
        }

        float moveX = input.Move.x;
        bool pushingIntoWall = player.IsPushingIntoWall(moveX);

        if (player.actionState != ActionState.Parry && !pushingIntoWall)
            motor.Move(moveX);
        else if (pushingIntoWall)
            motor.StopHorizontalMovement();

        motor.Tick(deltaTime, input.JumpHeld);
    }
}

public abstract class PlayerAction
{
    public abstract void TryRun(PlayerContext context);
}

public sealed class ParryPlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (context.Input.HasParryPressed && context.Defense.TryStartParry(context.Player))
            context.Input.ConsumeParryPressed();
    }
}

public sealed class DodgePlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (!context.Input.HasDashPressed || !context.Defense.TryStartDodge(context.Player))
            return;

        context.Motor.StartDodge(context.Defense.DodgeDashDuration);
        context.Input.ConsumeDashPressed();
    }
}

public sealed class JumpPlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (context.Input.HasJumpPressed && context.Player.TryJump())
            context.Input.ConsumeJumpPressed();
    }
}

public sealed class AttackPlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (context.Player.conditionState == ConditionState.Disarm)
            return;

        if (context.Input.HasAttackPressed && !context.SkillCaster.IsCasting && context.Combat.TryStartAttack(context.Player))
            context.Input.ConsumeAttackPressed();
    }
}

public sealed class SkillPlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (context.Player.conditionState == ConditionState.Disarm)
            return;

        if (context.Input.TryGetSkillPressed(out int skillIndex) && context.SkillCaster.TryCast(context.Player, context.Resources, skillIndex))
            context.Input.ConsumeSkillPressed();
    }
}

public sealed class InteractPlayerAction : PlayerAction
{
    public override void TryRun(PlayerContext context)
    {
        if (context.Input.HasInteractPressed && context.Interactor.TryInteract(context.Player))
            context.Input.ConsumeInteractPressed();
    }
}
