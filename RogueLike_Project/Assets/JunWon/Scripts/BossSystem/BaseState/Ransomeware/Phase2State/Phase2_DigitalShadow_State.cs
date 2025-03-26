using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // ��ȯ�� �׸��� ��
    private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 15f; // �׸��� �ڵ� �Ҹ� �ð�
    private float summonDistance = 5f;

    public Phase2_DigitalShadow_State(Ransomware owner) : base(owner)
    {
        owner.SetDigitalShadowState(this);
    }

    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_DigitalShadow_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("SummonShadows");
            if (owner.AbilityManager.UseAbility("SummonShadows"))
            {
                // ���� �׸��� ������ �ִϸ��̼� �̺�Ʈ�� ���� Ʈ���ŵ�
            }
        }
        else
        {
            // ������ �� ���� ��� ��� �Ϸ�
            isAttackFinished = true;
        }
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("SummonShadows");
    }

    public override void Interrupt()
    {
        base.Interrupt();

        // ���ͷ�Ʈ�� ��� Ȱ��ȭ�� �׸��� ����
        DestroyAllShadows();
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void SummonShadows()
    {
        if (owner.Player == null) return;

        // ���� �׸��� ����
        DestroyAllShadows();

        // �����Ƽ �Ŵ������� �׸��� ������ ��������
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null)
        {
            Debug.LogError("������ �׸��� �������� ã�� �� �����ϴ�!");
            return;
        }

        // �÷��̾� �ֺ��� �׸��ڸ� ��ȯ�� ��ġ ���
        for (int i = 0; i < shadowCount; i++)
        {
            // �������� ��ġ ���
            float angle = i * (360f / shadowCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // NavMesh ���� ��ġ ã��
            Vector3 targetPos = owner.Player.position + direction * summonDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, summonDistance, NavMesh.AllAreas))
            {
                // �׸��� �ν��Ͻ�ȭ
                GameObject shadow = GameObject.Instantiate(shadowPrefab, hit.position, Quaternion.identity);

                // �׸��� �ʱ�ȭ
                DigitalShadow shadowComponent = shadow.GetComponent<DigitalShadow>();
                if (shadowComponent != null)
                {
                    shadowComponent.Initialize(owner);
                    activeShadows.Add(shadow);

                    // �ı� �̺�Ʈ ���
                    shadowComponent.OnShadowDestroyed += HandleShadowDestroyed;
                }
            }
        }

        // ��� �׸��� �ı� Ȯ�� �ڷ�ƾ ����
        owner.StartCoroutine(CheckShadowsRoutine());
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);
        }

        // Ÿ�Ӿƿ� ���� ��� �׸��ڰ� �ı��Ǹ� �������� ��� ���� ����
        if (activeShadows.Count == 0)
        {
            owner.ApplyVulnerability(5f);
        }
    }

    private void DestroyAllShadows()
    {
        foreach (var shadow in activeShadows)
        {
            if (shadow != null)
            {
                // �ı� �� �̺�Ʈ ��� ����
                DigitalShadow shadowComponent = shadow.GetComponent<DigitalShadow>();
                if (shadowComponent != null)
                {
                    shadowComponent.OnShadowDestroyed -= HandleShadowDestroyed;
                }

                GameObject.Destroy(shadow);
            }
        }

        activeShadows.Clear();
    }

    private IEnumerator CheckShadowsRoutine()
    {
        // ���� �ð� ���� �׸��� Ȱ��ȭ ���
        yield return new WaitForSeconds(shadowLifetime);

        // ������� �Դµ� ���� �׸��ڰ� ������ ����
        if (activeShadows.Count > 0)
        {
            DestroyAllShadows();
        }

        // ���� ���� ����
        OnAttackFinished();
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}


