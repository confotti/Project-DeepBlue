using System;
using UnityEngine;

public class StateMachine<T1> where T1 : MonoBehaviour
{
    //public Action OnStateChanged;
    public State<T1> CurrentState { get; private set; }
    public State<T1> PreviousState { get; private set; }

    public void Initialize(State<T1> startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
        //OnStateChanged?.Invoke();
    }
    
    public void ChangeState(State<T1> newState)
    {
        if (newState == CurrentState) return;

        PreviousState = CurrentState;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        //OnStateChanged?.Invoke();
    }
}
