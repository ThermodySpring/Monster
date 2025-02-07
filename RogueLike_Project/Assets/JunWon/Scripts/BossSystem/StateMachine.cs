using System.Collections.Generic;
using System.Diagnostics;

public class StateMachine<T>
{
    public State<T> CurrentState { get; private set; }
    private List<Transition<T>> transitions = new List<Transition<T>>();

    public StateMachine(State<T> initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void AddTransition(Transition<T> transition)
    {
        transitions.Add(transition);
    }

    public void ForcedTransition(State<T> state)
    {
        if(state!=null)
        {
            CurrentState.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }
    }

    public void Update()
    {
        // ���� FSM ��ȯ ���� Ȯ��
        foreach (var transition in transitions)
        {
            if (transition.From == null ||
                (transition.From == CurrentState && transition.Condition()))
            {
                CurrentState.Exit();
                CurrentState = transition.To;
                CurrentState.Enter();
                break;
            }
        }

        // ���� ������ Update (CompositeState���, ���ο��� SubState�� ó��)
        CurrentState.Update();
        Debug.WriteLine(CurrentState.ToString());
    }
}