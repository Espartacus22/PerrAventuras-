using UnityEngine;

public class PState
{
    protected PlayerMovement player;
    protected PStateMachine stateMachine;

    protected PState(PlayerMovement player, PStateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void HandleInput() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
}
