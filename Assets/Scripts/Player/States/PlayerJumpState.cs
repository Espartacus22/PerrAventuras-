using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private PlayerLocal ctx;
    private bool usedDouble;

    public PlayerJumpState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        ctx.Jump();
        usedDouble = false;
    }

    public void Tick()
    {
        if (ctx == null || ctx.input == null) return;

        // Movimiento mientras está en el aire
        Vector2 moveInput = ctx.input.moveInput;
        ctx.Move(moveInput);

        // Permitir doble salto si está habilitado/desbloqueado
        if (ctx.input.jumpPressed)
        {
            if (ctx.isGrounded)
                ctx.Jump();
            else if (ctx.doubleJumpUnlocked || (ctx.characterData != null && ctx.characterData.canDoubleJump))
            {
                ctx.Jump();
                ctx.doubleJumpUnlocked = false;
            }
        }

        ctx.ApplyGravity();

        // Si aterriza, vuelve al estado de movimiento
        if (ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
    }

    public void Exit() { }
}
