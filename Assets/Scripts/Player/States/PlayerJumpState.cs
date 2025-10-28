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
        ctx.ApplyGravity();
        ctx.Move(ctx.input.GetMovement());

        // Doble salto
        if (ctx.input.GetJump() && ctx.canDoubleJump && !usedDouble)
        {
            ctx.Jump();
            ctx.canDoubleJump = false;
            usedDouble = true;
        }

        if (ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
        }
    }

    public void Exit() { }
}
