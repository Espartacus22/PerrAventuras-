using Fusion;
using UnityEngine;
using Networking;
using System.Collections;

public class NetPlayerAttack : NetworkBehaviour
{
    [Header("Attack")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private float attackCooldown = 0.35f;

    [Networked] private TickTimer AttackCooldownTimer { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetInputData>(out var inputData))
            return;

        if (inputData.buttons.IsSet(NetInputData.MOUSE_LEFT) && !AttackCooldownTimer.IsRunning)
        {
            AttackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);
            RPC_AttackFeedback();
        }

        if (AttackCooldownTimer.IsRunning && AttackCooldownTimer.Expired(Runner))
        {
            AttackCooldownTimer = TickTimer.None;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AttackFeedback()
    {
        StartCoroutine(AttackFlashRoutine());
    }

    private IEnumerator AttackFlashRoutine()
    {
        if (playerRenderer == null)
            yield break;

        Color originalColor = playerRenderer.material.color;
        playerRenderer.material.color = Color.yellow;

        yield return new WaitForSeconds(0.12f);

        playerRenderer.material.color = originalColor;
    }
}
