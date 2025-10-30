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
        if (ctx == null || ctx.input == null) return;

        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        // Jump input -> switch to JumpState
        if (ctx.input.GetJump() && ctx.isGrounded)
        {
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
            return;
        }

        // Dash input -> if enabled on character
        if (ctx.characterData != null && ctx.characterData.canDash && ctx.input.GetDash())
        {
            ctx.StateMachine.ChangeState(new PlayerDashState(ctx));
            return;
        }

        // Shooting from Move state (left click)
        if (Input.GetMouseButtonDown(0) && ctx.projectile != null)
        {
            ctx.projectile.Shoot();
        }

        ctx.ApplyGravity();
    }

    public void Exit() { }
}
