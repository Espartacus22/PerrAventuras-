using UnityEngine;

public class DefenseStrategy : IAttackStrategy
{
    public void Execute(ICompanionCombat combat, Transform target)
    {
        combat.Block(true);
    }
}
