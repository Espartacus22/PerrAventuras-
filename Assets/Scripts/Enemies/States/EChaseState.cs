using UnityEngine;

public class EChaseState : EState
{
    public EChaseState(EnemyController enemy, EStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void LogicUpdate()
    {
        if (enemy.Target == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        if (!enemy.IsTargetInDetectionRange())
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        if (enemy.IsTargetInAttackRange())
        {
            stateMachine.ChangeState(enemy.AttackState);
            return;
        }

        enemy.MoveToTarget();
    }
}
