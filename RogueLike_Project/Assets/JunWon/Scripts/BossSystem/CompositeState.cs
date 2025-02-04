using System.Collections.Generic;

public abstract class CompositeState<T> : State<T>
{
    protected State<T> currentSubState;
    protected List<Transition<T>> subTransitions = new List<Transition<T>>();

    public CompositeState(T owner) : base(owner) { }

    // ���� ���¸� �����ϴ� �޼���
    public void ChangeSubState(State<T> newSubState)
    {
        currentSubState?.Exit();
        currentSubState = newSubState;
        currentSubState.Enter();
    }

    // CompositeState ��ü�� Update -> ���� ������ Update�� ȣ��
    public override void Update()
    {
        base.Update();     // ���� ����(CompositeState)���� �������� ó���� ����
        currentSubState?.Update();

        // ���� ���� ��ȯ ���� Ȯ��
        foreach (var transition in subTransitions)
        {
            if (transition.From == currentSubState && transition.Condition())
            {
                ChangeSubState(transition.To);
                break;
            }
        }
    }
}