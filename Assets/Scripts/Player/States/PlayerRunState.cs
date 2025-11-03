using UnityEngine;

public class PlayerRunState : IPlayerState
{
    private PlayerLocal ctx;

    public PlayerRunState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        ctx.Run(true);
    }

    public void Tick()
    {
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (!ctx.input.GetRun())
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));

        if (ctx.input.GetShoot())
            ctx.Shoot();
        if (ctx.input.GetDash() && ctx.dashUnlocked)
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
        if (ctx.input.GetJump() && ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
    }

    public void Exit()
    {
        ctx.Run(false);
    }
}
