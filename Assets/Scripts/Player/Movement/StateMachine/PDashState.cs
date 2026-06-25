using UnityEngine;

public class PDashState : PState
{
    private float dashTimer;

    public PDashState(PlayerMovement player, PStateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        dashTimer = player.CharacterData.dashDuration;
        player.BeginDash();
    }

    public override void LogicUpdate()
    {
        // CORRECCIÓN MULTIPLAYER: Usamos el delta time del Runner de Fusion 
        // para que el tiempo corra sincronizado con las físicas de red.
        dashTimer -= player.Runner.DeltaTime;

        if (dashTimer <= 0f)
        {
            player.EndDash();

            if (!player.IsGrounded)
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }

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
        player.DashMove();
    }
}