using UnityEngine;

public class StateMachine
{
    private IPlayerState currentState;

    public void Initialize(IPlayerState startState)
    {
        currentState = startState;
        currentState.Enter();
    }

    public void ChangeState(IPlayerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Tick();
    }
}
