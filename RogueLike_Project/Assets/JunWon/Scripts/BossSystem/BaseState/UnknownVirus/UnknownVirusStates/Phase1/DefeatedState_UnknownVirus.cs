using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatedState_UnknownVirus : BaseState_UnknownVirus
{
    private float timer = 0f;
    private const float deathDuration = 5f;
    private bool deathEffectSpawned = false;

    public DefeatedState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
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
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // ��� ����Ʈ (1�� �� ����)
        if (!deathEffectSpawned && timer >= 1f)
        {
            // ��� ����Ʈ�� ���⿡ �߰�
            deathEffectSpawned = true;
        }

        // ��� ���� �ð��� ���� �� ������Ʈ ����
        if (timer >= deathDuration)
        {
            GameObject.Destroy(owner.gameObject);
        }
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Dead State ����");
    }
}
