using UnityEngine;

public class PCrouchState : PState
{
    public PCrouchState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.StartCrouch();
    }

    public override void Exit()
    {
        player.StopCrouch();
    }

    public override void LogicUpdate()
    {
        if (player.InputHandler.JumpPressed)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        if (player.InputHandler.CrouchReleased)
        {
            if (player.HasMovementInput())
                stateMachine.ChangeState(player.MoveState);
            else
                stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        player.MoveCrouched();
    }
}
