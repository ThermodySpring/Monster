using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class WormCombatState_UnknownVirus : BaseState_UnknownVirus
{
    // �����ڿ��� owner(UnknownVirusBoss) �Ҵ�
    public WormCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
    }

    public override void Enter()
    {
        // 1) ��ü(UnknownVirusBoss) �׺�޽� ������Ʈ ����
        owner.NmAgent.isStopped = true;
        // 2) ��ü �̵� �ִϸ��̼� ��
        owner.Animator.SetBool("IsMoving", false);

        // 3) �� �� �ν��Ͻ� Ȱ��ȭ�� TransformState���� �̹� ����Ǿ����Ƿ�,
        //    ���⼱ ���� �۾� ���� �� ������ FSM�� ��ü Update()�� ���ư����� ��.
    }

    public override void Update()
    {
        // �� ���� �پ��ִ� �ڽ� ������Ʈ(WormBossPrime)�� Update() �� fsm.Update() �ڵ� ȣ��
        // ���� ���� ����ξ �����մϴ�.
        // ���� ���� ��� �ʿ��ϴٸ� �Ʒ�ó�� ȣ���� ���� �ֽ��ϴ�:
        // var worm = owner.GetCurrentActiveBoss() as WormBossPrime;
        // worm?.ManualUpdate(); 
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
