using UnityEngine;

public class EPatrolState : EState
{
    public EPatrolState(EnemyController enemy, EStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void LogicUpdate()
    {
        if (enemy.IsTargetInDetectionRange())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        enemy.Patrol();
    }
}
