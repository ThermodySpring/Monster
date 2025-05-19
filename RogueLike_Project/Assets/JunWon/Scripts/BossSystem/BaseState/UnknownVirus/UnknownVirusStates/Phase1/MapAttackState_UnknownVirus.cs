using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapAttackState_UnknownVirus : BossPhaseBase<UnknownVirusBoss>
{
    private float stateTimer = 0f;
    private const float maxStateDuration = 12f;
    private bool isAttackFinished = false;

    public MapAttackState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        // ������ �ڽ��� �� ���� ���¸� �˰� ��
        owner.SetMapAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Map Attack State ����");
        stateTimer = 0f;
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
        stateTimer += Time.deltaTime;

        // �ִϸ��̼� �̺�Ʈ�� Ÿ�̸ӷ� �Ϸ� ó��
        if (stateTimer >= maxStateDuration)
        {
        }
        // (�߰�) �ִϸ��̼� �̺�Ʈ���� ���� ȣ���ص� �����ϴ�
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
