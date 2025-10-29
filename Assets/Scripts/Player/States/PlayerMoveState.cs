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
        Vector2 moveInput = ctx.input.moveInput;
        ctx.Move(moveInput);

        if (ctx.input.jumpPressed && ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));

        if (ctx.input.dashPressed)
            ctx.StartCoroutine(ctx.Dash());

        ctx.ApplyGravity();
    }

    public void Exit() { }
}
