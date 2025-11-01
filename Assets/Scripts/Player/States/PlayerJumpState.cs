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

        // Control aéreo
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        // Doble salto: solo si la habilidad está desbloqueada y aún no se usó
        bool canDouble = ctx.doubleJumpUnlocked || (ctx.characterData != null && ctx.characterData.canDoubleJump);

        // Detectar pulsación de salto (edge detection)
        bool jumpNow = ctx.input.GetJump() && !jumpPressedLastFrame;
        jumpPressedLastFrame = ctx.input.GetJump();

        if (jumpNow && canDouble && !ctx.hasDoubleJumped && !ctx.isGrounded)
        {
            ctx.Jump();
        }

        // Permitir dash en aire si está desbloqueado
        if (ctx.input.GetDash() && ctx.dashUnlocked)
        {
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
            return;
        }

        // Si aterrizó, volver a movimiento
        if (ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            return;
        }

        // Aplicar gravedad (ctx.ApplyGravity() normalmente se llama en Update del PlayerLocal,
        // pero si tu flujo requiere, puedes llamarlo aquí también)
        // ctx.ApplyGravity();
    }

    public void Exit() { }
}
