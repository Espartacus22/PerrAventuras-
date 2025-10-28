using UnityEngine;

public class PlayerMoveState : IPlayerState
{
    private  PlayerLocal ctx;

    public PlayerMoveState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter() { }

    public void Tick()
    {
        if (ctx == null || ctx.input == null) return;

        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (ctx.input.GetJump() && ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
        }

        ctx.ApplyGravity();
    }

    public void Exit() { }
}
