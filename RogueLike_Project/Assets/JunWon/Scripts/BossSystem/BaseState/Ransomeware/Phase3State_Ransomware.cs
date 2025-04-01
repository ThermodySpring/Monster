using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase3State_Ransomware : BossPhaseBase<Ransomware>
{
    private Phase2_DigitalShadow_State digitalShadowState;
    private bool isPhaseInitialized = false;

    public Phase3State_Ransomware(Ransomware owner) : base(owner)
    {
        digitalShadowState = new Phase2_DigitalShadow_State(owner);
    }

    public override void Enter()
    {
        Debug.Log("�������� ���� ������3 (�߾� ����) ����");

        // �ߺ� ���� ����
        if (isPhaseInitialized) return;
        isPhaseInitialized = true;

        // ������3 �ִϸ��̼� ���̾� Ȱ��ȭ
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase2"), 1);
        owner.Animator.SetLayerWeight(owner.Animator.GetLayerIndex("Phase1"), 1);

        // �����Ƽ �ʱ�ȭ
        InitializeAbility();

        // ȿ���� ��� �� �ð� ȿ�� �߰� (�ʿ� ��)
        PlayPhaseTransitionEffects();

        // �߾� ���� ����
        owner.StartCoroutine(StartLastStandPatternWithDelay());
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

    private void PlayPhaseTransitionEffects()
    {
        // ������ ��ȯ ȿ�� ���� �߰� (�ʿ� ��)
        // ��: ȭ�� ������, ���� �ֺ� ����Ʈ ��

        // �ִϸ��̼� Ʈ���� ����
        owner.Animator.SetTrigger("SummonShadows");
    }

    private IEnumerator StartLastStandPatternWithDelay()
    {
        // �߾� ���� ���� �� ª�� ���� (�ִϸ��̼� ȿ���� ����)
        yield return new WaitForSeconds(1.5f);

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
