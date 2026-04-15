using Fusion;
using UnityEngine;
using Networking;
using System.Collections;

public class NetPlayerAttack : NetworkBehaviour
{
    [Header("Attack")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Transform atkPoint;
    [SerializeField] private float attackRadius = 1.25f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private LayerMask playerLayer;

    [Networked] private TickTimer AttackCooldownTimer { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NetInputData>(out var inputData))
            return;

        if (inputData.buttons.IsSet(NetInputData.MOUSE_LEFT) && !AttackCooldownTimer.IsRunning)
        {
            AttackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);

            RPC_AttackFeedback();

            if (HasStateAuthority)
            {
                TryHitPlayers();
            }
        }

        if (AttackCooldownTimer.IsRunning && AttackCooldownTimer.Expired(Runner))
        {
            AttackCooldownTimer = TickTimer.None;
        }
    }

    private void TryHitPlayers()
    {
        if (atkPoint == null)
            return;

        Collider[] hits = Physics.OverlapSphere(atkPoint.position, attackRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            NetPlayerHealth health = hit.GetComponentInParent<NetPlayerHealth>();

            if (health == null)
                continue;

            if (health.Object == Object)
                continue;

            Debug.Log($"Hit player: {health.Object.name}");
            health.TakeDamage(damageAmount);
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

    private void OnDrawGizmosSelected()
    {
        if (atkPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(atkPoint.position, attackRadius);
    }

}
