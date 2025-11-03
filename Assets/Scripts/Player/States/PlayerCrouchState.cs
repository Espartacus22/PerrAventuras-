using UnityEngine;

public class PlayerCrouchState : IPlayerState
{
    private PlayerLocal ctx;

    public PlayerCrouchState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        ctx.Crouch(true);
    }

    public void Tick()
    {
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (!ctx.input.GetCrouch())
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));

        if (ctx.input.GetShoot())
            ctx.Shoot();
        if (ctx.input.GetJump() && ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
    }

    public void Exit()
    {
        ctx.Crouch(false);
    }
}
