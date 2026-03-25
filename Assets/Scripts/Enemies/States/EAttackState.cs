using UnityEngine;

public class EAttackState : EState
{
    public EAttackState(EnemyController enemy, EStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
    }

    public override void LogicUpdate()
    {
        if (enemy.Target == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        if (!enemy.IsTargetInAttackRange())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        enemy.LookAtTarget();
        enemy.Attack();
    }
}
