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
    bool enabledTurret = true;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
    }

    void Update()
    {
        if (!enabledTurret)
        {
            line.enabled = false;
            return;
        }

        UpdateTarget();

        if (currentTarget == null)
        {
            line.enabled = false;
            return;
        }

        AimAtTarget();
        FireLaser();
    }

    void UpdateTarget()
    {
        currentTarget = null;

        // Buscar jugador/es en rango
        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        if (hits.Length == 0) return;

        // Elegir el más cercano
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

        // Igual que en el dron: calculamos vector 3D hacia el jugador (un poco más arriba)
        Vector3 toTarget = (currentTarget.position + Vector3.up * 1.2f) - head.position;
        if (toTarget.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(toTarget.normalized);
        head.rotation = Quaternion.Slerp(head.rotation, lookRot, rotateSpeed * Time.deltaTime);
    }

    void FireLaser()
    {
        RaycastHit hit;
        Vector3 dir = firePoint.forward; // ahora forward apunta EXACTO al player

        if (Physics.Raycast(firePoint.position, dir, out hit, range))
        {
            // Dibujar láser
            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);

            // Aplicar daño, aunque el collider sea hijo del player
            PlayerLevel lvl = hit.collider.GetComponentInParent<PlayerLevel>();
            if (lvl != null)
            {
                float deltaDamage = damagePerSecond * Time.deltaTime;
                lvl.TakeDamage(Mathf.RoundToInt(deltaDamage));
            }
        }
        else
        {
            line.enabled = false;
        }
    }

    // Llamá esto desde EnergyCore cuando se destruye
    public void DisableTurret()
    {
        enabledTurret = false;
        line.enabled = false;
    }
}
