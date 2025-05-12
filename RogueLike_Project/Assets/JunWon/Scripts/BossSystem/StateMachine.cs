using System;
using System.Collections.Generic;
using System.Diagnostics;

public class StateMachine<T>
{
    public State<T> CurrentState { get; private set; }
    private List<Transition<T>> transitions = new List<Transition<T>>();
    private List<GlobalTransition<T>> globalTransitions = new List<GlobalTransition<T>>();

    public StateMachine(State<T> initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void AddTransition(Transition<T> transition)
    {
        transitions.Add(transition);
    }

    public void AddGlobalTransition(State<T> to, Func<bool> condition, List<State<T>> exceptStates = null)
    {
        globalTransitions.Add(new GlobalTransition<T>(to, condition, exceptStates));
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
        // 1. ���� global transition Ȯ��
        foreach (var globalTransition in globalTransitions)
        {
            // ���� ���°� ���� ���� ��Ͽ� ���� ������ �����Ǹ� ��ȯ
            if (!globalTransition.IsStateExcepted(CurrentState) && globalTransition.Condition())
            {
                CurrentState.Exit();
                CurrentState = globalTransition.To;
                CurrentState.Enter();
                return; // ��ȯ �Ϸ� �� ����
            }
        }

        // 2. �������� �Ϲ� ���� ��ȯ Ȯ��
        foreach (var transition in transitions)
        {
            if (transition.From == CurrentState && transition.Condition())
            {
                CurrentState.Exit();
                CurrentState = transition.To;
                CurrentState.Enter();
                return; // ��ȯ �Ϸ� �� ����
            }
        }

        // ��ȯ�� ������ ���� ���� ������Ʈ
        CurrentState.Update();
        Debug.WriteLine(CurrentState.ToString());
    }
}