using UnityEngine;
using System.Collections;

public class PlayerDashState : IPlayerState
{
    private PlayerLocal ctx;
    private bool finished;

    public PlayerDashState(PlayerLocal context)
    {
        ctx = context;
    }

    public void Enter()
    {
        finished = false;
        // Inicia la corrutina del dash en el contexto (PlayerLocal)
        if (ctx != null)
            ctx.StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        // Obtiene dirección preferida por input; si no hay input usa forward del player
        Vector2 moveInput = ctx.input != null ? ctx.input.GetMovement() : Vector2.zero;
        Vector3 dir = ctx.GetDirectionFromMovement(moveInput);
        float elapsed = 0f;
        float duration = ctx.dashDuration;
        float speed = ctx.dashSpeed;

        while (elapsed < duration)
        {
            // mover solo la componente horizontal; la vertical la maneja PlayerLocal (ApplyGravity)
            Vector3 move = new Vector3(dir.x, 0f, dir.z) * speed * Time.deltaTime;
            ctx.controller.Move(move);
            elapsed += Time.deltaTime;
            yield return null;
        }

        finished = true;
    }

    public void Tick()
    {
        // permitir disparo mientras dashing si querés
        if (ctx.input != null && ctx.input.GetShoot())
            ctx.Shoot();

        if (finished)
        {
            // volver al estado apropiado
            if (ctx.isGrounded)
                ctx.StateMachine.ChangeState(new PlayerMoveState(ctx));
            else
                ctx.StateMachine.ChangeState(new PlayerJumpState(ctx));
        }
    }

    public void Exit()
    {
        // nada especial por ahora
    }
}
