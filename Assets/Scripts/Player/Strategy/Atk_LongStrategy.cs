using UnityEngine;

public class Atk_LongStrategy : IAttackStrategy
{
    public void Execute(ICompanionCombat combat, Transform target)
    {
        combat.Ranged(target);
    }
}
