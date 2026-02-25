using UnityEngine;

public class Attack : MonoBehaviour
{
    public int damage = 10;
    public float attackSpeed = 1f;
    private float nextAttackTime = 0f;

    public void PerformAttack(Transform target)
    {
        if (Time.time >= nextAttackTime)
        {
            HealthSystem targetHealth = target.GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Ataque realizado: {damage} de daño al objetivo {target.name}");
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
            else
            {
                Debug.LogWarning($"El objetivo {target.name} no tiene un componente HealthSystem.");
            }
        }
    }
}
