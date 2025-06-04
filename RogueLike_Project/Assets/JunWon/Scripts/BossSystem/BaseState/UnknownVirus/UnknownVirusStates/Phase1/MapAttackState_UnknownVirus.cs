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
        VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
        isAttackFinished = false;

        // �̵� ����
        owner.NmAgent.isStopped = true;
        owner.Animator.SetBool("IsMoving", false);

        owner.StartCoroutine(ExecuteSequentialAttack());


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

    private IEnumerator ExecuteSequentialAttack()
    {
        if (owner.basic != null)
        {
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx == null)
                vfx = owner.basic.AddComponent<VirusCubeAttackEffect>();

            // 1. ���̷��� ť�� ������ ���� ����
            vfx.StartLaserAttack();

            // 2. ���� �Ϸ���� ��� (3.6��)
            yield return new WaitForSeconds(3.2f);

            // 3. �� ���� ����
            if (owner.AbilityManager.UseAbility("MapAttack"))
            {
                owner.TriggerMapAttack();
            }
        }

        // 4. ���� �Ϸ�
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
