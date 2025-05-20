using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    private bool isTransformationComplete = false;
    private float stateEntryTime;
    private float transformDecisionDelay = 1.0f;
    private BossForm targetForm;

    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner)
    {
        owner.SetTransformState(this);
    }

    #region LifeCycle
    public override void Enter()
    {
        Debug.Log("[TransformState_UnknownVirus] Enter");

        // ���� �ʱ�ȭ
        isTransformationComplete = false;
        stateEntryTime = Time.time;

        // �ִϸ��̼� Ʈ���� ����
        if (owner.Animator != null)
        {
            owner.Animator.SetTrigger("Transform");
        }

        // ���� �� ȿ�� ���� (������)
        ApplyTransformationEffects();
    }

    public override void Update()
    {
        // ������ �Ϸ�Ǿ����� �ƹ��͵� ���� ����
        if (isTransformationComplete) return;

        // �� ������ TransformRoutine �ڷ�ƾ���� ó��
        // ���⼭�� �߰� ������ ó��
    }

    public override void Exit()
    {
        // �ִϸ��̼� Ʈ���� ����
        if (owner.Animator != null)
        {
            owner.Animator.ResetTrigger("Transform");
        }

        // ���� ȿ�� ����
        CleanupTransformationEffects();
    }
    #endregion

    #region TransfomrFunc
    public void OnTransformationComplete()
    {
        isTransformationComplete = true;
    }

    
    private void ApplyTransformationEffects()
    {
        // ��: ��ƼŬ ȿ��, ���� ��
    }

    private void CleanupTransformationEffects()
    {
        // ���� ���� ȿ�� ����
    }

    public bool IsTransformationComplete()
    {
        return isTransformationComplete;
    }
    #endregion
}