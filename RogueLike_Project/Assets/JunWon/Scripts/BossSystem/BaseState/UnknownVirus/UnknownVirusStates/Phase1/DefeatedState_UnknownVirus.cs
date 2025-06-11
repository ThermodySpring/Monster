using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DefeatedState_UnknownVirus : BaseState_UnknownVirus
{
    [Header("��� ����")]
    [SerializeField] private DeathFragmentSystem fragmentSystem;

    public DefeatedState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        fragmentSystem = owner.basic.GetComponent<DeathFragmentSystem>();

        Debug.Log("UnknownVirus: Dead State ����");

        // ���� ���� ���� ���� ���� �ߴ�
        InterruptCurrentState();

        // ��� ���� ���� ��Ȱ��ȭ
        owner.ApplyForm(UnknownVirusBoss.BossForm.Basic);

        // ������Ʈ ��Ȱ��ȭ
        if (owner.NmAgent != null)
        {
            owner.NmAgent.isStopped = true;
            owner.NmAgent.enabled = false;
        }

        // ��� �̺�Ʈ �߻�
        EventManager.Instance.TriggerMonsterKilledEvent(true);

        // ��� �ִϸ��̼�
        if (owner.Animator != null)
        {
            owner.Animator.SetTrigger("Death");
        }

        owner.StartCoroutine(ExecuteSequentialDeath());
    }

    public override void Update()
    {
        // �⺻ ������Ʈ ����
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Dead State ����");
    }

    /// <summary>
    /// ���� ���� ���� ���¸� ������ �ߴ�
    /// </summary>
    private void InterruptCurrentState()
    {
        // FSM�� ���� ���°� MapAttackState���� Ȯ���ϰ� �ߴ�
        var currentState = owner.Fsm.CurrentState;

        if (currentState is MapAttackState_UnknownVirus mapAttackState)
        {
            Debug.Log("MapAttack ���� �� ��� - ���� ���� �ߴ�");
            mapAttackState.Interrupt();
        }
        else if (currentState is TransformState_UnknownVirus transformState)
        {
            Debug.Log("Transform ���� �� ��� - ���� ���� �ߴ�");
            transformState.Interrupt();
        }

        // ��� �ڷ�ƾ �ߴ�
        owner.StopAllCoroutines();

        // ���� ���� ť�� ���� ȿ�� ��� �ߴ�
        StopAllActiveEffects();
    }

    /// <summary>
    /// ��� Ȱ��ȭ�� ȿ�� �ߴ�
    /// </summary>
    private void StopAllActiveEffects()
    {
        if (owner.basic != null)
        {
            // ť�� ���� ȿ�� �ߴ� - ���� ��ġ ����
            VirusCubeAttackEffect vfx = owner.basic.GetComponent<VirusCubeAttackEffect>();
            if (vfx != null)
            {
                vfx.SetReturnMode(false); // ���� ��ġ�� ���ư��� ����
                vfx.StopEffect();
                Debug.Log("[DeathState] ť�� ȿ�� �ߴ� - ���� ��ġ ����");
            }
        }

        // �÷��� ȿ�� �ߴ�
        if (owner.FLOATINGEFFECT != null)
        {
            owner.FLOATINGEFFECT.SetPaused(true);
        }
    }

    private void HandleDeath()
    {
        // ���� ����߸��� ���� ����
        if (fragmentSystem != null)
        {
            fragmentSystem.TriggerDeathFragmentation();
        }
        else
        {
            Debug.LogWarning("DeathFragmentSystem�� �����ϴ�.");
        }

        Debug.Log("[UnknownVirusBoss] ��� - ���� ����߸��� ���� ����");
    }

    private IEnumerator ExecuteSequentialDeath()
    {
        HandleDeath();
        yield return new WaitForSeconds(2.0f);
        owner.gameObject.SetActive(false);
    }
}