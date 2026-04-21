using Fusion;
using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayerAttack : NetworkBehaviour
{
    [Header("Melee Attack")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Transform atkPoint;
    [SerializeField] private float attackRadius = 1.25f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Projectile Attack")]
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileForwardOffset = 1.2f;
    [SerializeField] private float projectileUpOffset = 1.1f;

    [Networked] private TickTimer AttackCooldownTimer { get; set; }

    private Material runtimeMaterial;
    private Color baseColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (playerRenderer != null)
        {
            runtimeMaterial = playerRenderer.material;
            baseColor = runtimeMaterial.color;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (AttackCooldownTimer.IsRunning && AttackCooldownTimer.Expired(Runner))
        {
            AttackCooldownTimer = TickTimer.None;
        }

        if (!GetInput<NetworkInputPlayer>(out var inputData))
            return;

        if (!HasInputAuthority)
            return;

        if (inputData.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_0) && !AttackCooldownTimer.IsRunning)
        {
            Vector3 attackCenter = transform.position + Vector3.up * 1.0f + transform.forward * 1.2f;

            RPC_RequestMeleeAttack(attackCenter);
        }

        if (inputData.buttons.IsSet(NetworkInputPlayer.MOUSE_BUTTON_1) && !AttackCooldownTimer.IsRunning)
        {
            RPC_RequestProjectileAttack();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestMeleeAttack(Vector3 attackCenter)
    {
        Debug.Log($"RPC_RequestMeleeAttack from player {Object.InputAuthority.PlayerId} at {attackCenter}");
        if (AttackCooldownTimer.IsRunning)
            return;

        AttackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);

        RPC_AttackFeedback();
        TryHitPlayers(attackCenter);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestProjectileAttack()
    {
        if (AttackCooldownTimer.IsRunning)
            return;

        AttackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);

        RPC_AttackFeedback();
        ShootProjectile();
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null)
            return;

        Vector3 spawnPos;
        Vector3 shootDir;

        if (shootPoint != null)
        {
            // usamos el forward del player para mayor estabilidad
            spawnPos = transform.position + Vector3.up * projectileUpOffset + transform.forward * projectileForwardOffset;
            shootDir = transform.forward;
        }
        else
        {
            spawnPos = transform.position + Vector3.up * projectileUpOffset + transform.forward * projectileForwardOffset;
            shootDir = transform.forward;
        }

        NetworkObject proj = Runner.Spawn(
            projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(shootDir),
            Object.InputAuthority
        );

        NetProjectile projectile = proj.GetComponent<NetProjectile>();
        if (projectile != null)
        {
            projectile.Init(shootDir, Object.InputAuthority);
        }

        Debug.Log($"Projectile spawned by {Object.InputAuthority.PlayerId} at {spawnPos} dir {shootDir}");
    }

    private void TryHitPlayers(Vector3 attackCenter)
    {
        Collider[] hits = Physics.OverlapSphere(attackCenter, attackRadius, playerLayer);

        Debug.Log($"TryHitPlayers center: {attackCenter}");
        Debug.Log($"Hits found: {hits.Length}");

        HashSet<NetworkObject> damagedPlayers = new HashSet<NetworkObject>();

        foreach (Collider hit in hits)
        {
            NetPlayerHealth health = hit.GetComponentInParent<NetPlayerHealth>();

            if (health == null)
                continue;

            if (health.Object == Object)
                continue;

            if (health.Object == null)
                continue;

            if (damagedPlayers.Contains(health.Object))
                continue;

            damagedPlayers.Add(health.Object);

            Debug.Log($"Hit player: {health.Object.name}");
            health.TakeDamage(damageAmount);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AttackFeedback()
    {
        if (runtimeMaterial == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(AttackFlashRoutine());
    }

    private IEnumerator AttackFlashRoutine()
    {
        if (runtimeMaterial == null)
            yield break;

        runtimeMaterial.color = Color.yellow;
        yield return new WaitForSeconds(0.12f);

        if (runtimeMaterial != null)
            runtimeMaterial.color = baseColor;

        flashRoutine = null;
    }

    private void OnDisable()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (atkPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 debugCenter = transform.position + Vector3.up * 1.0f + transform.forward * 1.2f;
            Gizmos.DrawWireSphere(atkPoint.position, attackRadius);
        }
    }
}
