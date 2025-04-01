using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class DigitalShadow : RangedMonster
{

    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Repulsion Settings")]
    [SerializeField] private float repulsionDistance = 2.5f; // �ٸ� �׸��ڿ��� �ּ� �Ÿ�
    [SerializeField] private float repulsionStrength = 3.0f; // �ݹ߷� ����
    [SerializeField] private float jitterAmount = 0.5f;       // �ణ�� ���� �̵� ����

    // �÷��̾ ���� �߰��ϴ� ���� ��ħ ������ ���� UpdateChase �������̵�
    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;

        // �ֺ��� �ٸ� DigitalShadow����� �ݹ߷� ���
        Vector3 repulsion = CalculateRepulsion();

        // �ణ�� ���� ����(�����ο� ������ �ο�)
        Vector3 jitter = new Vector3(Random.Range(-jitterAmount, jitterAmount), 0, Random.Range(-jitterAmount, jitterAmount));

        // ���� �������� �÷��̾� ��ġ�� �ݹ� ���Ϳ� ���͸� ���� ��
        Vector3 destination = target.position + repulsion + jitter;
        nmAgent.SetDestination(destination);

        // �÷��̾�� ��������� ���� ���·� ��ȯ
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }
    }

    // �ֺ��� �ٸ� DigitalShadow���� �Ÿ��� ����Ͽ� �ݹ� ���͸� ��ȯ
    private Vector3 CalculateRepulsion()
    {
        Vector3 repulsion = Vector3.zero;
        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        int count = 0;

        foreach (var shadow in shadows)
        {
            if (shadow == this) continue;
            float distance = Vector3.Distance(transform.position, shadow.transform.position);
            if (distance < repulsionDistance && distance > 0f)
            {
                // �Ÿ��� �������� ���ϰ� �о���� (������ ���)
                Vector3 pushDir = (transform.position - shadow.transform.position).normalized;
                repulsion += pushDir * (repulsionStrength / distance);
                count++;
            }
        }

        if (count > 0)
        {
            repulsion /= count;
        }

        return repulsion;
    }

    protected override void UpdateDie()
    {
        // ���� ���� ó�� ���� ����
        base.UpdateDie();

        // �̺�Ʈ�� ���� �������� �׾����� �˸�
        OnShadowDestroyed?.Invoke(gameObject);
    }
}

