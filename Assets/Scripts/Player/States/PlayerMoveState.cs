using UnityEngine;

public class PlayerMoveState : IPlayerState
{
    private PlayerLocal ctx;

    public PlayerMoveState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter() { }

    public void Tick()
    {
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (ctx.input.GetRun())
            ctx.StateMachine.ChangeState(new PlayerRunState(ctx));

        if (ctx.input.GetCrouch())
            ctx.StateMachine.ChangeState(new PlayerCrouchState(ctx));

        if (ctx.input.GetJump() && ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));

        if (ctx.input.GetDash() && ctx.dashUnlocked)
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));

        if (ctx.input.GetShoot())
            ctx.Shoot();
    }

    public void Exit() { }
}
