using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private bool isAttackFinished = false;

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // ������ �ڽ��� �� ���� ���¸� �˰� ��
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State ����");
        isAttackFinished = false;

        // �̵� ����
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        if (owner.AbilityManager.UseAbility("MapAttack"))
        {
            owner.TriggerMapAttack();

        }


        // ���� �ִϸ��̼� & ȿ��
        owner.Animator.SetTrigger("MapAttack");
    }

    public override void Update()
    {
       
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Map Attack State ����");
        // �̵� �簳
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("MapAttack");
    }

    /// <summary>�ִϸ��̼� �̺�Ʈ�� ���� Ÿ�̸� ���� �� ȣ��</summary>
    public void OnAttackFinished()
    {
        if (isAttackFinished) return;
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
