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
        ctx.currentSpeed = ctx.crouchSpeed;

        // Reduce altura del CharacterController para simular agacharse
        ctx.controller.height = 1.0f;
        ctx.controller.center = new Vector3(0, 0.5f, 0);
    }

    public void Tick()
    {
        Vector2 moveInput = ctx.input.GetMovement();
        ctx.Move(moveInput);

        if (!ctx.input.GetCrouch())
        {
            ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
        }

        if (ctx.input.GetJump())
        {
            ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
        }
    }

    public void Exit()
    {
        // Vuelve a la altura normal
        ctx.controller.height = 2.0f;
        ctx.controller.center = new Vector3(0, 1.0f, 0);
        ctx.currentSpeed = ctx.moveSpeed;
    }
}
