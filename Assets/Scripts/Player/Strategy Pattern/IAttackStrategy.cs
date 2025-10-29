using UnityEngine;

public interface IAttackStrategy
{
    void Execute(ProjectileLocal shooter, Transform target = null);
}
