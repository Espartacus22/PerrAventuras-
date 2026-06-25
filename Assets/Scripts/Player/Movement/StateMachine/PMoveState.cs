using UnityEngine;

public class PMoveState : PState
{
    public PMoveState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void LogicUpdate()
    {
        // 1. PRIORIDAD ABSOLUTA: Si el jugador presiona Dash o Salto de forma voluntaria,
        // cambiamos de estado al instante sin importar si el terreno parpadeó.
        if (player.InputHandler.DashPressed)
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        if (player.InputHandler.JumpPressed)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        // 2. AGARRE DEL SUELO: Si de verdad no estás tocando el piso (y no apretaste nada),
        // pasás al estado de salto/caída normal.
        if (!player.IsGrounded)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        // 3. RESTO DE TRANSICIONES HORIZONTALES
        if (player.InputHandler.CrouchPressed)
        {
            stateMachine.ChangeState(player.CrouchState);
            return;
        }

        if (!player.HasMovementInput())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (player.InputHandler.RunHeld)
        {
            stateMachine.ChangeState(player.RunState);
        }
    }

    public override void PhysicsUpdate()
    {
        player.Move(false);
    }
}