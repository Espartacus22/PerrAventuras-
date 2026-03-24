using UnityEngine;
using UnityEngine.Playables;

public class PStateMachine
{
    public PState CurrentState { get; private set; }

    public void Initialize(PState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PState newState)
    {
        if (CurrentState != null)
            CurrentState.Exit();

        CurrentState = newState;
        CurrentState.Enter();
    }
}
