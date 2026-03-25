using UnityEngine;

public class EStateMachine
{
    public EState CurrentState { get; private set; }

    public void Initialize(EState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
