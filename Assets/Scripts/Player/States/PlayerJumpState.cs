using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private PlayerLocal ctx;
    private bool jumpPressedLastFrame;

    public PlayerJumpState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        if (ctx == null) return;
        // Ejecuta el primer salto al entrar al estado
        ctx.Jump();
        jumpPressedLastFrame = false;
    }

    public void Tick()
    {
        if (ctx == null || ctx.input == null) return;

        // Control a�reo
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        // Doble salto: solo si la habilidad est� desbloqueada y a�n no se us�
        bool canDouble = ctx.doubleJumpUnlocked || (ctx.characterData != null && ctx.characterData.canDoubleJump);

        // Detectar pulsaci�n de salto (edge detection)
        bool jumpNow = ctx.input.GetJump() && !jumpPressedLastFrame;
        jumpPressedLastFrame = ctx.input.GetJump();

        if (jumpNow && canDouble && !ctx.hasDoubleJumped && !ctx.isGrounded)
        {
            ctx.Jump();
        }

        // Permitir dash en aire si est� desbloqueado
        if (ctx.input.GetDash() && ctx.dashUnlocked)
        {
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
            return;
        }

        // Si aterriz�, volver a movimiento
        if (ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            return;
        }

        // Aplicar gravedad (ctx.ApplyGravity() normalmente se llama en Update del PlayerLocal,
        // pero si tu flujo requiere, puedes llamarlo aqu� tambi�n)
        // ctx.ApplyGravity();
    }

    public void Exit() { }
}
