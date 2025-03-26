using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase3State_Ransomware : BossPhaseBase<Ransomware>
{
    private Phase2_DigitalShadow_State digitalShadowState;

    public Phase3State_Ransomware(Ransomware owner) : base(owner)
    {
        digitalShadowState = new Phase2_DigitalShadow_State(owner);
    }

    public override void Enter()
    {
        Debug.Log("�������� ���� ������3 (�߾� ����) ����");

        // ������3 �ִϸ��̼� ���̾� Ȱ��ȭ (�ʿ��� ���)
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase1"), 1);

        // �����Ƽ �ʱ�ȭ
        InitializeAbility();

        // �߾� ���� ���� - ��� �п� ����
        StartLastStandPattern();
    }

    private void InitializeAbility()
    {
        // �߾� ���Ͽ��� ����� �ɷ� Ȱ��ȭ
        owner.AbilityManager.SetAbilityActive("SummonShadow");

        // �ٸ� ��� �ɷ� ��Ȱ��ȭ
        owner.AbilityManager.SetAbilityInactive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityInactive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityInactive("DataBlink");
        owner.AbilityManager.SetAbilityInactive("Lock");
    }

    private void StartLastStandPattern()
    {
        Debug.Log("�������� ���� �߾� ���� (������ ������ �п�) ����");

        // ���� �̵� ����
        owner.NmAgent.isStopped = true;

        // Ư�� ȿ��/ī�޶� ���� �� �߰� ����

        // �п� ���� ����
        digitalShadowState.Enter();
        digitalShadowState.ActivateLastStandSplit();
    }

    public override void Update()
    {
        // �߾� ���Ͽ����� ������ ������ ���¸� ������Ʈ
        digitalShadowState.Update();
    }

    public override void Exit()
    {
        // �߾� ���� ���� - ������ ������ ���� ����
        digitalShadowState.Exit();
    }

    public override void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;

        // ������ ������ ���� �ߴ�
        digitalShadowState.Interrupt();

        // ������3 ���� ���� �۾�
        owner.AbilityManager.SetAbilityInactive("SummonShadow");

        // ���� �̵� ���� �� ȸ�� ���
        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }
}
