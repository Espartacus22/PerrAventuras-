using UnityEngine;

public class StateMachine
{
    private IPlayerState currentState;

    public void Initialize(IPlayerState startingState)
    {
        currentState = startingState;
        currentState?.Enter();
    }

    public void ChangeState(IPlayerState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Tick();
    }
}
