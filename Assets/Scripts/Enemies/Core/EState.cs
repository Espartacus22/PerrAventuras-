using UnityEngine;

public abstract class EState
{
    protected EnemyController enemy;
    protected EStateMachine stateMachine;

    protected EState(EnemyController enemy, EStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void LogicUpdate() { }
}
