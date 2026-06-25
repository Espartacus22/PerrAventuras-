using UnityEngine;

public class PJumpState : PState
{
    private bool _hasAppliedInitialJump;

    public PJumpState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        // Si entramos porque presionó Espacio, hace el primer salto de forma legítima
        if (player.InputHandler.JumpPressed)
        {
            player.Jump();
            _hasAppliedInitialJump = true;
        }
        else
        {
            // Si entramos porque voló por inercia, se cayó de una plataforma o por la loma,
            // NO salta de forma automática. Entra en caída libre.
            _hasAppliedInitialJump = false;
        }
    }

    public override void LogicUpdate()
    {
        // Doble salto controlado de forma estricta por el contador físico
        if (player.InputHandler.JumpPressed)
        {
            // Si el player ya saltó una vez (JumpCount == 1) y tiene permitido el doble salto
            if (player.HasDoubleJump() && player.JumpCount == 1)
            {
                player.Jump();
            }
        }

        // Dash desde el aire
        if (player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // Transición al tocar suelo de verdad
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