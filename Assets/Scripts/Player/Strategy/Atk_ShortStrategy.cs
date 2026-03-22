using UnityEngine;

public class Atk_ShortStrategy : IAttackStrategy
{
    public void Execute(ICompanionCombat combat, Transform target)
    {
        combat.MeleeShort(target);
    }
}
