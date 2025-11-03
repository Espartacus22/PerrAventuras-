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
        ctx.Jump();
    }

    public void Tick()
    {
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (ctx.input.GetShoot())
            ctx.Shoot();

        if (ctx.input.GetDash() && ctx.dashUnlocked)
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));

        // Si cae al suelo → Move
        if (ctx.isGrounded)
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
    }

    public void Exit() { }
}
