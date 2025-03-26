using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // �п��� ���� ��
    [SerializeField] private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 20f; // �п� ���� ���� �ð�
    private float summonDistance = 8f;
    private Vector3 originalPosition; // ������ ���� ��ġ ����
    private Quaternion originalRotation; // ������ ���� ȸ�� ����
    private Vector3 originalScale; // ������ ���� ũ�� ����
    private bool isLastStandMode = false; // �߾� ��� �÷���

    public Phase2_DigitalShadow_State(Ransomware owner) : base(owner)
    {
        owner.SetDigitalShadowState(this);
    }

    

    public override void Enter()
    {
        owner.AbilityManager.SetAbilityActive("SummonShadows");

        isAttackFinished = false;
        Debug.Log("[Phase2 SummonState] Enter");
        owner.NmAgent.isStopped = true;
        originalPosition = owner.transform.position;
        originalRotation = owner.transform.rotation;
        originalScale = owner.transform.localScale;

        if (CanExecuteAttack())
        {
            owner.Animator.SetTrigger("SummonShadow");
            if (owner.AbilityManager.UseAbility("SummonShadow"))
            {
                // ���� �п��� �ִϸ��̼� �̺�Ʈ�� ���� Ʈ���ŵ�
            }
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

        owner.GetComponent<MonsterStatus>().SetHealth(0);
    }

    public void ActivateLastStandSplit()
    {
        if (owner.Player == null) return;

        // �߾� ��� Ȱ��ȭ
        isLastStandMode = true;

        // ���� �׸��� ����
        DestroyAllShadows();

        // ��ü�� ���� (��Ȱ��ȭ�� ���� ���� �������� ����)
        HideOwner();

        // �п� ���� ����
        SpawnSplitFragments();

        // ��� �׸��� �ı� Ȯ�� �ڷ�ƾ ����
        owner.StartCoroutine(CheckShadowsRoutine());
    }

    private void HideOwner()
    {
        // ��ü�� �������� �ݶ��̴��� ��Ȱ��ȭ
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // NavMeshAgent ��Ȱ��ȭ
        owner.NmAgent.enabled = false;
    }

    private void RestoreOwner()
    {
        // ��ü ��ġ ����
        owner.transform.position = originalPosition;
        owner.transform.rotation = originalRotation;
        owner.transform.localScale = originalScale;

        // �������� �ݶ��̴� Ȱ��ȭ
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        Collider[] colliders = owner.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // NavMeshAgent Ȱ��ȭ
        owner.NmAgent.enabled = true;
    }

    private void SpawnSplitFragments()
    {
        // �����Ƽ �Ŵ������� �׸���/�п� ���� ������ ��������
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null)
        {
            Debug.LogError("�п� ���� �������� ã�� �� �����ϴ�!");
            return;
        }

        // �������� ���� �п� ���� ��ġ
        for (int i = 0; i < shadowCount; i++)
        {
            // �������� ��ġ ���
            float angle = i * (360f / shadowCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // NavMesh ���� ��ġ ã��
            Vector3 targetPos = originalPosition + direction * summonDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, summonDistance, NavMesh.AllAreas))
            {
                // �п� ���� �ν��Ͻ�ȭ
                GameObject fragment = GameObject.Instantiate(shadowPrefab, hit.position, Quaternion.identity);

                // ���� ���� (��ü�� ���������� �ణ �ٸ���)
                CustomizeSplitFragment(fragment);

                // �п� ���� �ʱ�ȭ
                DigitalShadow fragmentComponent = fragment.GetComponent<DigitalShadow>();
                if (fragmentComponent != null)
                {
                    fragmentComponent.Initialize(owner);
                    fragmentComponent.SetAsLastStandFragment(isLastStandMode); // �߾� ���Ͽ� �������� ǥ��
                    activeShadows.Add(fragment);

                    // �ı� �̺�Ʈ ���
                    fragmentComponent.OnShadowDestroyed += HandleShadowDestroyed;
                }
            }
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    private void CustomizeSplitFragment(GameObject fragment)
    {
        // �߾� ���Ͽ����� �п� ���� ���� Ŀ���͸���¡
        // ��: ���� ����, ����Ʈ �߰�, ũ�� ���� ��
        Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // ������ �ణ �ٸ��� (��: ���� �迭��)
            if (renderer.material != null)
            {
                renderer.material.color = new Color(
                    renderer.material.color.r + 0.2f,
                    renderer.material.color.g - 0.1f,
                    renderer.material.color.b - 0.1f,
                    renderer.material.color.a
                );
            }
        }

        // ũ�⸦ ��ü���� �ణ �۰�
        fragment.transform.localScale = originalScale * 0.8f;

        // ��ƼŬ �ý��� �߰� (�ִ� ���)
        ParticleSystem particleSystem = fragment.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = new Color(1f, 0.5f, 0.5f); // ���� �迭 ��ƼŬ
        }
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);

            // �α� �߰�
            Debug.Log($"�׸��� �ı���. ���� �׸���: {activeShadows.Count}, �߾� ���: {isLastStandMode}");
        }

        // ��� �п� ������ �ı��Ǿ����� Ȯ��
        if (activeShadows.Count == 0)
        {
            if (isLastStandMode)
            {
                // �߾� ��忡�� ��� �п� ������ �ı��Ǹ� ���� óġ
                Debug.Log("��� �߾� ��� �п� ������ �ı���. ���� óġ.");
                MonsterStatus status = owner.GetComponent<MonsterStatus>();
                status.SetHealth(0); // ���� ü�� ��� ����
            }
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

        // �߾� ����� ��� ���� óġ ó��
        if (isLastStandMode)
        {
            Debug.Log("�߾� ��� �ð� ����. ���� óġ.");
            MonsterStatus status = owner.GetComponent<MonsterStatus>();
            status.SetHealth(0);
        }
        else
        {
            // �Ϲ� ��忡���� ���� ���� ó��
            Debug.Log("�п� ���� �ð� ����. ��ü ����.");
            RestoreOwner();
            OnAttackFinished();
        }
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}


