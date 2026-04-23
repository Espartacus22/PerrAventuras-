using UnityEngine;
using Fusion;

[RequireComponent(typeof(LineRenderer))]
public class LaserTurret : NetworkBehaviour
{
    [Header("Referencias")]
    public Transform head;
    public Transform firePoint;

    [Header("Parámetros")]
    public float range = 25f;
    public float rotateSpeed = 5f;
    public int damagePerSecond = 20;
    public LayerMask playerMask;

    [Networked] public NetworkBool isTurretEnabled { get; set; }

    private LineRenderer line;
    private float _damageAccumulator;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
    }

    void OnValidate()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (line != null)
        {
            line.enabled = false;
            line.positionCount = 0;
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority) isTurretEnabled = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!isTurretEnabled)
        {
            line.enabled = false;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        if (hits.Length == 0)
        {
            line.enabled = false;
            return;
        }

        // Buscar al más cercano
        Transform target = hits[0].transform;
        float bestDist = Vector3.Distance(transform.position, target.position);
        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector3.Distance(transform.position, hits[i].transform.position);
            if (d < bestDist) { bestDist = d; target = hits[i].transform; }
        }

        // --- PUNTERÍA 3D CORREGIDA ---
        // Apuntamos al pecho del jugador para que el láser no pase por arriba
        Vector3 targetCenter = target.position + Vector3.up * 1.2f;
        Vector3 fullDir = (targetCenter - head.position).normalized;

        if (fullDir.sqrMagnitude > 0.001f)
        {
            // Ahora la cabeza de la torreta puede rotar hacia abajo
            Quaternion lookRot = Quaternion.LookRotation(fullDir);
            head.rotation = Quaternion.Slerp(head.rotation, lookRot, rotateSpeed * Runner.DeltaTime);
        }

        // El raycast sale hacia donde mira la cabeza (head.forward)
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, head.forward, out hit, range))
        {
            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);

            if (HasStateAuthority && hit.collider.CompareTag("Player"))
            {
                PlayerLevel lvl = hit.collider.GetComponent<PlayerLevel>();
                if (lvl != null)
                {
                    _damageAccumulator += damagePerSecond * Runner.DeltaTime;
                    if (_damageAccumulator >= 1f)
                    {
                        int damageToApply = Mathf.FloorToInt(_damageAccumulator);
                        _damageAccumulator -= damageToApply;
                        lvl.TakeDamage(damageToApply);
                    }
                }
            }
        }
        else
        {
            line.enabled = false;
        }
    }

    public void DisableTurret()
    {
        if (HasStateAuthority) isTurretEnabled = false;
        line.enabled = false;
    }
}