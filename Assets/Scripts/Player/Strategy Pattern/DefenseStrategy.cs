using UnityEngine;

public class DefenseStrategy : IAttackStrategy
{
    public void Execute(ProjectileLocal shooter, Transform target = null)
    {
        // No dispara, podría activar animación de bloqueo
    }
}
