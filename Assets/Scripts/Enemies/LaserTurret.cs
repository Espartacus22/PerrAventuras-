using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTurret : MonoBehaviour
{
    [Header("Referencias")]
    public Transform head;        // parte que gira
    public Transform firePoint;   // punto donde nace el láser

    [Header("Parámetros")]
    public float range = 25f;
    public float rotateSpeed = 5f;
    public int damagePerSecond = 20;
    public LayerMask playerMask;

    bool enabledTurret = true;
    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;

        // opcional: darle un ancho
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
    }

    void Update()
    {
        if (!enabledTurret)
        {
            line.enabled = false;
            return;
        }

        // Buscar players en rango
        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerMask);
        if (hits.Length == 0)
        {
            line.enabled = false;
            return;
        }

        // Tomar el más cercano
        Transform target = hits[0].transform;
        float bestDist = Vector3.Distance(transform.position, target.position);
        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector3.Distance(transform.position, hits[i].transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                target = hits[i].transform;
            }
        }

        // Rotar la cabeza hacia el player (solo en XZ)
        Vector3 toTarget = (target.position + Vector3.up * 1.2f) - head.position;
        Vector3 flatDir = new Vector3(toTarget.x, 0, toTarget.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(flatDir.normalized);
            head.rotation = Quaternion.Slerp(head.rotation, lookRot, rotateSpeed * Time.deltaTime);
        }

        // Disparar el láser (raycast + daño continuo)
        RaycastHit hit;
        Vector3 dir = head.forward;

        if (Physics.Raycast(firePoint.position, dir, out hit, range))
        {
            line.enabled = true;
            line.SetPosition(0, firePoint.position);
            line.SetPosition(1, hit.point);

            if (hit.collider.CompareTag("Player"))
            {
                PlayerLevel lvl = hit.collider.GetComponent<PlayerLevel>();
                if (lvl != null)
                {
                    float deltaDamage = damagePerSecond * Time.deltaTime;
                    lvl.TakeDamage(Mathf.RoundToInt(deltaDamage));
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
        enabledTurret = false;
        line.enabled = false;
        // acá podés cambiar material, apagar luz, etc.
    }
}
