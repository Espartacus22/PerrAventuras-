using UnityEngine;
using System.Collections;

public class PlayerDashState : IPlayerState
{
    private PlayerLocal ctx;
    private bool isDashing;

    public PlayerDashState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        if (ctx == null) return;
        isDashing = true;
        ctx.StartCoroutine(DashRoutine());
    }

    public void Tick()
    {
        // Mientras dashing se dejó la lógica en la corrutina.
        // Aquí revisamos transiciones y permitimos disparo si quierés.
        if (!isDashing)
        {
            if (ctx.isGrounded)
                ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            else
                ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
        }

        if (Input.GetMouseButtonDown(0) && ctx.projectile != null)
            ctx.projectile.Shoot();
    }

    public void Exit()
    {
        isDashing = false;
    }

    private IEnumerator DashRoutine()
    {
        // Usa los valores de ctx (asegurate que public float dashDuration/dashSpeed existen)
        float duration = ctx.dashDuration;
        float speed = ctx.dashSpeed;

        // Dirección: preferir input -> si no, forward
        Vector3 dir = (ctx.input != null) ? ctx.input.GetMoveDirectionRelativeToCamera(ctx.input.GetMovement()) : ctx.transform.forward;
        if (dir.sqrMagnitude < 0.001f) dir = ctx.transform.forward;
        dir.y = 0;
        dir.Normalize();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            ctx.controller.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
