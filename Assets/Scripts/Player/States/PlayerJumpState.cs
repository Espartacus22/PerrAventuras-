using UnityEngine;

public class PlayerJumpState : IPlayerState
{
    private PlayerLocal ctx;

    public PlayerJumpState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        ctx.jumpCount++;

        // Aplicar salto
        ctx.verticalVelocity = Mathf.Sqrt(ctx.jumpForce * -2f * ctx.gravity);

        // Aplicar animación futuro (placeholder)
        // if(ctx.Animator != null) ctx.Animator.SetTrigger("Jump");
    }

    public void Tick()
    {
        ctx.CheckGround();

        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        ctx.verticalVelocity += ctx.gravity * Time.deltaTime;
        ctx.controller.Move(new Vector3(0, ctx.verticalVelocity, 0) * Time.deltaTime);

        if (ctx.isGrounded)
        {
            ctx.verticalVelocity = 0;

            if (ctx.input.GetMovement().sqrMagnitude > 0.01f)
                ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
        }

        if (ctx.input.GetJump() && ctx.doubleJumpUnlocked && ctx.jumpCount < 2)
            ctx.Jump();

        if (ctx.input.GetDash() && ctx.dashUnlocked)
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
    }

    public void Exit() { }
}
