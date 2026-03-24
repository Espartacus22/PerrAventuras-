using UnityEngine;

public class PRunState : PState
{
    public PRunState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void LogicUpdate()
    {
        if (!player.IsGrounded)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        if (player.InputHandler.CrouchPressed)
        {
            stateMachine.ChangeState(player.CrouchState);
            return;
        }

        if (player.InputHandler.JumpPressed)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (!player.HasMovementInput())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (!player.InputHandler.RunHeld)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate()
    {
        player.Move(true);
    }
}
