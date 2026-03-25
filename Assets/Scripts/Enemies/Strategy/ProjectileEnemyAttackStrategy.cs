using UnityEngine;

public class ProjectileEnemyAttackStrategy : IEnemyAttackStrategy
{
    public void Execute(EnemyController enemy)
    {
        if (enemy.Target == null) return;
        if (enemy.ProjectilePrefab == null || enemy.FirePoint == null) return;

        Vector3 dir = (enemy.Target.position + Vector3.up * 1.2f - enemy.FirePoint.position).normalized;

        GameObject projectile = Object.Instantiate(
            enemy.ProjectilePrefab,
            enemy.FirePoint.position,
            Quaternion.LookRotation(dir)
        );

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * enemy.ProjectileSpeed;
        }

        EnemyProjectileBehavior proj = projectile.GetComponent<EnemyProjectileBehavior>();
        if (proj != null)
        {
            proj.SetDamage(enemy.AttackDamage);
        }

        Debug.Log($"{enemy.name} disparó proyectil al player.");
    }
}
