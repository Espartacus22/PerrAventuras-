using UnityEngine;

public class ManualAttackStrategy : IAttackStrategy
{
    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        if (shooter == null) return;
        if (Input.GetMouseButtonDown(0))
            shooter.Shoot();
    }
}
