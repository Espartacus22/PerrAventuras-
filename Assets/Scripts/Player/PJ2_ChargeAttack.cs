using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PJ2_ChargeAttack : MonoBehaviour
{
    [Header("Charge Settings")]
    public float chargeForce = 18f;
    public float chargeDuration = 0.35f;
    public float damage = 30f;
    public float cooldown = 2f;

    [Header("Hit Settings")]
    public float hitRadius = 1.1f;
    public string enemyTag = "Enemy";

    private bool isCharging;
    private bool isUnlocked;            // se desbloquea por PlayerLevel (UnlockCharge)
    private float lastChargeTime;
    private Rigidbody rb;

    // Para no pegarle 30 veces al mismo enemigo en una sola carga
    private readonly HashSet<EnemyStats> hitThisCharge = new HashSet<EnemyStats>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isUnlocked = false; // arranca bloqueado; se habilita con UnlockCharge()
    }

    /// <summary>
    /// PlayerLevel lo llama al llegar al nivel requerido.
    /// </summary>
    public void UnlockCharge()
    {
        isUnlocked = true;
        // opcional: para que puedas usarla instantáneo apenas se desbloquea
        lastChargeTime = -999f;
        Debug.Log("PJ2_ChargeAttack: Charge UNLOCKED");
    }

    public void ExecuteCharge()
    {
        if (!isUnlocked)
        {
            Debug.Log("PJ2_ChargeAttack: intento de usar charge, pero NO está desbloqueado.");
            return;
        }

        if (isCharging) return;
        if (Time.time < lastChargeTime + cooldown) return;

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        lastChargeTime = Time.time;
        hitThisCharge.Clear();

        float startTime = Time.time;
        Vector3 dir = transform.forward;

        while (Time.time < startTime + chargeDuration)
        {
            // Unity 6 suele usar linearVelocity. Para compatibilidad, seteo ambas.
            Vector3 v = new Vector3(dir.x * chargeForce, rb.linearVelocity.y, dir.z * chargeForce);
            rb.linearVelocity = v;
            rb.linearVelocity = v;

            // Dano por overlap durante el dash (más confiable que OnCollisionEnter)
            Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
            foreach (Collider h in hits)
            {
                if (!h.CompareTag(enemyTag)) continue;

                EnemyStats enemy = h.GetComponent<EnemyStats>();
                if (enemy == null) continue;

                if (hitThisCharge.Contains(enemy)) continue;

                hitThisCharge.Add(enemy);
                enemy.TakeDamage(Mathf.RoundToInt(damage));
            }

            yield return null;
        }

        isCharging = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
