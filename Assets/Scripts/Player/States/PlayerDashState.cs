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
        // mientras dash se ejecuta por corrutina no hacemos control normal
        // pero permitimos disparar si se desea
        if (Input.GetMouseButtonDown(0) && ctx.projectile != null)
        {
            ctx.Shoot();
        }

        // si la corrutina terminó, volver al estado apropiado
        if (!isDashing)
        {
            if (ctx.isGrounded) ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            else ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
        }
    }

    public void Exit()
    {
        isDashing = false;
    }

    private IEnumerator DashRoutine()
    {
        // determino dirección preferida por input/cámara
        Vector2 m = ctx.input != null ? ctx.input.GetMovement() : Vector2.zero;
        Vector3 dir = (m.sqrMagnitude > 0.001f) ? ctx.input.GetMoveDirectionRelativeToCamera(m) : ctx.transform.forward;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) dir = ctx.transform.forward;
        dir.Normalize();

        float elapsed = 0f;
        float duration = ctx.dashDuration;
        float speed = ctx.dashSpeed;

        while (elapsed < duration)
        {
            ctx.controller.Move(dir * speed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
