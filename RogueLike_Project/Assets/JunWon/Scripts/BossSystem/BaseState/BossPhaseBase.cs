using UnityEngine;

public abstract class BossPhaseBase<T> : State<T>
{
    // ������
    public BossPhaseBase(T owner) : base(owner)
    {
    }

    // �ʿ��ϴٸ�, ����Ǵ� ������ ������ ���⿡ �ۼ�
    public override void Enter()
    {
        // ������ ���� ��
        Debug.Log($"Enter BossPhaseBase<{typeof(T).Name}>");
    }

    public override void Update()
    {
        // ����(������)���� �������� ó���� ����
        // ��: HP üũ, ������ ��ȯ ���� ��
    }

    public override void Exit()
    {
        Debug.Log($"Exit BossPhaseBase<{typeof(T).Name}>");
    }
}
