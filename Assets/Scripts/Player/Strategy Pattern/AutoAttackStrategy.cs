using UnityEngine;

public class AutoAttackStrategy : IAttackStrategy
{
    public float attackRange = 10f;
    private float nextCheckTime = 0f;
    private float checkInterval = 0.2f;

    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        if (shooter == null || target == null) return;

        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

        float dist = Vector3.Distance(shooter.transform.position, target.position);
        if (dist <= attackRange)
        {
            // Orientar al objetivo (opcional)
            Vector3 dir = (target.position - shooter.transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                shooter.transform.rotation = Quaternion.LookRotation(dir);

            shooter.Shoot();
        }
    }
}
