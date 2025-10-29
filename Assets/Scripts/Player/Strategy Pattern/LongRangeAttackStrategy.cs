using UnityEngine;

public class LongRangeAttackStrategy : IAttackStrategy
{
    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        if (shooter == null) return;
        shooter.Shoot();
    }
}
