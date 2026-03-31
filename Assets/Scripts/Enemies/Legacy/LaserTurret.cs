using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTurret : MonoBehaviour
{
    [Header("Referencias")]
    public Transform head;
    public Transform firePoint;

    [Header("Parámetros")]
    public float range = 25f;
    public float rotateSpeed = 5f;
    public int damagePerSecond = 20;
    public LayerMask playerMask;

    LineRenderer line;
    Transform currentTarget;
    bool isActive = true;   // <- ESTADO DE LA TORRETA

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }
    }

    void Update()
    {
        // Si la torreta está apagada, no hace NADA
        if (!isActive)
        {
            if (line != null && line.enabled)
                line.enabled = false;
            return;
        }

        UpdateTarget();

        if (currentTarget == null)
        {
            if (line != null && line.enabled)
                line.enabled = false;
            return;
        }

        AimAtTarget();
        FireLaser();
    }

    void UpdateTarget()
    {
        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        if (hits.Length == 0) return;

        float bestDist = Mathf.Infinity;
        for (int i = 0; i < hits.Length; i++)
        {
            float d = Vector3.Distance(transform.position, hits[i].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                currentTarget = hits[i].transform;
            }
        }
    }

    void AimAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 toTarget = (currentTarget.position + Vector3.up * 1.2f) - head.position;
        if (toTarget.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(toTarget.normalized);
        head.rotation = Quaternion.Slerp(head.rotation, lookRot, rotateSpeed * Time.deltaTime);
    }

    void FireLaser()
    {
        if (line == null) return;

        RaycastHit hit;
        Vector3 dir = firePoint.forward;

        if (Physics.Raycast(firePoint.position, dir, out hit, range))
        {
            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);

            PlayerLevel lvl = hit.collider.GetComponentInParent<PlayerLevel>();
            if (lvl != null)
            {
                int damageThisFrame = Mathf.Max(1, Mathf.RoundToInt(damagePerSecond * Time.deltaTime));
                lvl.TakeDamage(damageThisFrame);
            }
        }
        else
        {
            line.enabled = false;
        }
    }

    // Llamado por EnergyCore
    public void DisableTurret()
    {
        isActive = false;

        if (line != null)
            line.enabled = false;

        // también podemos desactivar cualquier collider que tenga
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
