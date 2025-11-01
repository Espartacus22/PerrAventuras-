using UnityEngine;

public class PlayerRunState : IPlayerState
{
    private PlayerLocal ctx;

    public PlayerRunState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        if (ctx == null) return;
        // Setear la velocidad de run usando el método público del player (si querés)
        ctx.currentSpeed = ctx.moveSpeed * ctx.runMultiplier;
    }

    public void Tick()
    {
        if (ctx == null || ctx.input == null) return;

        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        // Si deja de apretar run, volver al MoveState
        if (!ctx.input.GetRun())
        {
            // Restaurar velocidad en Exit() o aquí
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            return;
        }

        // Si salta mientras corre, ir a JumpState
        if (ctx.input.GetJump() && ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
            return;
        }

        // Permitir disparo o dash mientras corre
        if (ctx.input.GetShoot())
            ctx.Shoot();

        if (ctx.input.GetDash() && ctx.dashUnlocked)
        {
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
            return;
        }
    }

    public void Exit()
    {
        // Restaurar velocidad de movimiento normal
        if (ctx != null) ctx.currentSpeed = ctx.moveSpeed;
    }
}
