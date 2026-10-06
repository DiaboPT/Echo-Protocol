using System;

public class FiniteStateMachine {
    public IState CurrentState { get; private set; }

    public void Initialize(IState startingState) {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(IState newState) {
        CurrentState?.Exit();

        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Update() {
        CurrentState?.Update();
    }
}
