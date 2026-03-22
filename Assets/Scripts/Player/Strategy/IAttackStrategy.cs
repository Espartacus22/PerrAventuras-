using UnityEngine;

public interface IAttackStrategy
{
    void Execute(ICompanionCombat combat, UnityEngine.Transform target);
}
