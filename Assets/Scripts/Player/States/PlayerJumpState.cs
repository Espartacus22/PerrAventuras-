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
        // perform initial jump
        if (ctx == null) return;

        // allow jump only if under max jumps
        if (ctx.jumpCount < ctx.maxJumps)
            ctx.Jump();
    }

    public void Tick()
    {
        if (ctx == null || ctx.input == null) return;

        // air control
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        // double-jump
        if (ctx.input.GetJump() && ctx.jumpCount < ctx.maxJumps)
        {
            ctx.Jump();
        }

        // dash in air if allowed
        if (ctx.characterData != null && ctx.characterData.canDash && ctx.input.GetDash())
        {
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
            return;
        }

        // fall back to move state when grounded
        if (ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            return;
        }

        ctx.ApplyGravity();
    }

    public void Exit() { }
}
