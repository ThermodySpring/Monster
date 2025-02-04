using UnityEngine;


public abstract class IntroState<T> : State<T>
{
    public IntroState(T owner) : base(owner)
    {
    }

    // �ʿ��ϴٸ�, ����Ǵ� ������ ������ ���⿡ �ۼ�
    public override void Enter()
    {
        // ������ ���� ��
        Debug.Log($"Enter IntroPhase<{typeof(T).Name}>");
    }

    public override void Update()
    {
    }

    public override void Exit()
    {
    }
}
