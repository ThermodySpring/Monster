using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatedState_UnknownVirus : BaseState_UnknownVirus
{
    private float timer = 0f;
    private const float deathDuration = 5f;
    private bool deathEffectSpawned = false;

    [Header("��� ����")]
    [SerializeField] private DeathFragmentSystem fragmentSystem;

    public DefeatedState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        fragmentSystem = owner.basic.GetComponent<DeathFragmentSystem>();

        Debug.Log("UnknownVirus: Dead State ����");
        timer = 0f;
        deathEffectSpawned = false;

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

       
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Dead State ����");
    }

    private void HandleDeath()
    {
        // ���� ��� ó��...

        // ���� ����߸��� ���� ����
        if (fragmentSystem != null)
        {
            fragmentSystem.TriggerDeathFragmentation();
        }
        else
        {
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
