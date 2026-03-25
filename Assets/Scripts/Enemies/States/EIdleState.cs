using UnityEngine;

public class EIdleState : EState
{
    public EIdleState(EnemyController enemy, EStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
    }

    public override void LogicUpdate()
    {
        if (enemy.IsTargetInDetectionRange())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        if (enemy.patrolPoints != null && enemy.patrolPoints.Length > 0)
        {
            stateMachine.ChangeState(enemy.PatrolState);
        }
    }
}
