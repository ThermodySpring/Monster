using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FieldMage : MonsterBase
{
    [Header("Field Settings")]
    [SerializeField] protected float maintainDistance = 20f;          // �÷��̾�� ������ �ּ� �Ÿ�
    [SerializeField] protected float fieldSpawnInterval = 10f;     // �ʵ� ���� ����(��)
    protected float fieldSpawnTimer = 0f;                          // Ÿ�̸�

    [SerializeField] private GameObject debuffFieldPrefab;         // �÷��̾� ��ġ�� �� ����� �ʵ�
    [SerializeField] private GameObject buffFieldPrefab;           // ���� ��ġ�� �� ���� �ʵ�


    [SerializeField]
    private LayerMask monsterLayer; // ���� Layer
    [SerializeField]
    private LayerMask playerLayer; // �÷��̾� Layer


    protected override void Start()
    {
        base.Start();
        // stateActions�� CAST ���¿� ���� �޼��� ���
        stateActions[State.CAST] = UpdateCast;
        stateActions.Remove(State.ATTACK);
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }
        nmAgent.isStopped = false;
        nmAgent.speed = chaseSpeed;
        nmAgent.SetDestination(target.position);

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer <= maintainDistance)
        {
            ChangeState(State.CAST);
        }
    }

    private void UpdateCast()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        nmAgent.isStopped = true;

        fieldSpawnTimer += Time.deltaTime;
        if (fieldSpawnTimer >= fieldSpawnInterval)
        {
            PlaceBuffField();
            fieldSpawnTimer = 0f;
        }

        // �ʿ信 ���� CAST ���¿��� �ٸ� ���·� ��ȯ�ϴ� ���� �߰�
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer > maintainDistance)
        {
            ChangeState(State.CHASE);
        }
    }

    protected virtual void PlaceDebuffField()
    {
        if (debuffFieldPrefab != null && target != null)
        {
            // �÷��̾� ��ġ�� ����� �ʵ带 ����
            Instantiate(debuffFieldPrefab, target.position, Quaternion.identity);
        }
    }

    protected virtual void PlaceBuffField()
    {
        if (buffFieldPrefab != null)
        {
            // 1. �ֺ� ���� Ž�� (�ݰ� 10 ����)
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, monsterLayer); // ���� ���̾�
            Debug.Log("������ �������� " + colliders.Length + "���� ");

            // 2. ü���� ���� ���� ���� ã�� (�ڽ� ����)
            MonsterStatus lowestHealthMonster = null;
            float lowestHealth = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                MonsterStatus monster = collider.GetComponent<MonsterStatus>();
                if (monster != null)
                {
                    if (monster.GetHealth() < lowestHealth)
                    {
                        lowestHealth = monster.GetHealth();
                        lowestHealthMonster = monster;
                    }
                }
            }

            // �ڽŵ� ����
            if (monsterStatus.GetHealth() < lowestHealth)
            {
                lowestHealthMonster = monsterStatus;
            }

            // 3. ã�� ���� �Ǵ� �ڽ��� ��ġ�� ���� �ʵ� ����
            if (lowestHealthMonster != null)
            {
                Debug.Log("���� ����!!");
                Instantiate(buffFieldPrefab, lowestHealthMonster.transform.position, Quaternion.identity);
            }
        }
    }
}
