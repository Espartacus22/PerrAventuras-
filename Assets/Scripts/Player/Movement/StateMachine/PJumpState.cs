using UnityEngine;

public class PJumpState : PState
{
    public PJumpState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Jump();
    }

    public override void LogicUpdate()
    {
        if (player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        if (player.IsGrounded && player.VerticalVelocity <= 0.05f)
        {
            if (player.HasMovementInput())
            {
                if (player.InputHandler.RunHeld)
                    stateMachine.ChangeState(player.RunState);
                else
                    stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        player.MoveInAir();
    }
}
