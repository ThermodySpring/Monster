using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class RansomwareCombatState_UnknownVirus : BaseState_UnknownVirus
{
    // �����ڿ��� owner(UnknownVirusBoss) �Ҵ�
    public RansomwareCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        // 1) ��ü(UnknownVirusBoss) �׺�޽� ������Ʈ ����
        owner.NmAgent.isStopped = true;
        // 2) ��ü �̵� �ִϸ��̼� ��
        owner.Animator.SetBool("IsMoving", false);

       
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        // �� ���� ��ȯ �� ��ü ���͸� ���� �׺�޽� ������Ʈ ��Ȱ��ȭ
        owner.NmAgent.isStopped = false;
    }

    public override void Interrupt()
    {
        // �ʿ� �� ���ͷ�Ʈ ���� �߰�
        base.Interrupt();
    }
}
