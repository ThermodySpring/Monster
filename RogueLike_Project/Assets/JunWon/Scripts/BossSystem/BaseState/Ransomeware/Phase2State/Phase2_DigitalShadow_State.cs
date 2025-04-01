using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DigitalShadow_State : BossPhaseBase<Ransomware>
{
    private bool isAttackFinished = false;
    private int shadowCount = 6; // �п��� ���� ��
    [SerializeField] private List<GameObject> activeShadows = new List<GameObject>();
    private float shadowLifetime = 60f; // �п� ���� ���� �ð�
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
        GameObject shadowPrefab = owner.Shadow;
        if (shadowPrefab == null) return;

        Debug.Log($"������ ���� ����: ��ǥ {shadowCount}��");
        float spawnRadius = 5f;

        for (int i = 0; i < shadowCount; i++)
        {
            float angle = (i * (360f / shadowCount));
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            float distance = spawnRadius * 0.8f;
            Vector3 targetPos = originalPosition + direction * distance;

            NavMeshHit hit;
            GameObject fragment;
            if (NavMesh.SamplePosition(targetPos, out hit, distance, NavMesh.AllAreas))
            {
                fragment = Object.Instantiate(shadowPrefab, hit.position, Quaternion.identity);
                Debug.Log($"������ {i + 1} ������: ��ġ {hit.position}");
            }
            else
            {
                fragment = Object.Instantiate(shadowPrefab,
                    originalPosition + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)),
                    Quaternion.identity);
                Debug.Log($"������ {i + 1} ��ü ��ġ�� ������");
            }

            DigitalShadow fragmentComponent = fragment.GetComponent<DigitalShadow>();
            if (fragmentComponent != null)
            {
                // �п� ������ �׾��� �� �������� ��ȣ�� �����ϵ��� �̺�Ʈ ����
                fragmentComponent.OnShadowDestroyed += HandleShadowDestroyed;
            }
            activeShadows.Add(fragment);
        }

        owner.ShadowsSpawned = true;
        Debug.Log($"���� �Ϸ�: �� {activeShadows.Count}�� ������");
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    private void CustomizeSplitFragment(GameObject fragment)
    {
        // �߾� ���Ͽ����� �п� ���� ���� Ŀ���͸���¡
        // ������ �п� ������ �ణ�� �ٸ��� ����� �����ϱ� ���� ��
        Renderer[] renderers = fragment.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            // ������ �ణ �ٸ��� (��: ���� �迭�� - �� �п�ü���� �ٸ� ����)
            if (renderer.material != null)
            {
                float hueVariation = Random.Range(-0.1f, 0.1f);
                renderer.material.color = new Color(
                    Mathf.Clamp01(renderer.material.color.r + 0.2f + hueVariation),
                    Mathf.Clamp01(renderer.material.color.g - 0.1f - hueVariation),
                    Mathf.Clamp01(renderer.material.color.b - 0.1f + hueVariation),
                    renderer.material.color.a
                );
            }
        }

        // ũ�⸦ �ణ�� �ٸ��� (�� �ڿ������� �п� ����)
        float sizeVariation = Random.Range(0.75f, 0.9f);
        fragment.transform.localScale = originalScale * sizeVariation;

        // ��ƼŬ �ý��� �߰� (�ִ� ���)
        ParticleSystem particleSystem = fragment.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;

            // ���� �ణ �ٸ� ������ ��ƼŬ
            float r = Random.Range(0.8f, 1.0f);
            float g = Random.Range(0.3f, 0.6f);
            float b = Random.Range(0.3f, 0.6f);
            main.startColor = new Color(r, g, b); // ���� �迭 ��ƼŬ

            // ��ƼŬ ũ�⵵ ����ȭ
            float sizeMult = Random.Range(0.9f, 1.1f);
            main.startSize = main.startSize.constant * sizeMult;
        }

        // RangedMonster ������Ʈ�� �߰� ���� (�پ��� ���� ����)
        RangedMonster rangedMonster = fragment.GetComponent<RangedMonster>();
        if (rangedMonster != null)
        {
            // ���� ��ٿ�, ���� �ð� ���� �ణ�� �ٸ��� ����
            //rangedMonster.attack = rangedMonster.attackCooldown * Random.Range(0.8f, 1.2f);
        }
    }

    private void HandleShadowDestroyed(GameObject shadow)
    {
        if (shadow == null) return;

        if (activeShadows.Contains(shadow))
        {
            activeShadows.Remove(shadow);
            Debug.Log($"�׸��� �ı���. ���� �׸���: {activeShadows.Count}");
        }

        activeShadows.RemoveAll(item => item == null);

        if (activeShadows.Count == 0)
        {
            Debug.Log("��� �п� ������ �ı���. �������� ��ȣ ����.");
            // �������� �п� ���� �ı� �Ϸ� ��ȣ ���� (��: ���� ������� ��ȯ)
            owner.DigitalShadowsFinished(); // Ransomware �Ǵ� ���� Ŭ������ �� �޼��带 ����
            RestoreOwner();
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


