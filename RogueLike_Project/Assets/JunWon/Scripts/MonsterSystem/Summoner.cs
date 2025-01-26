using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Summoner : MonsterBase
{
    [Header("Field Settings")]
    [SerializeField] protected float maintainDistance = 10f;          // �÷��̾�� ������ �ּ� �Ÿ�
    [SerializeField] protected float SummonTimeInterval = 5f;     // �ʵ� ���� ����(��)
    [SerializeField] protected float MaxSummonCount = 5;     
    protected float SummonTimer = 0f;                          // Ÿ�̸�

    [SerializeField] private GameObject[] summonedEnemies;
    private List<GameObject> totalEnemies = new List<GameObject>();
    private int currentSummonCount = 0;

    bool hasCast = false;


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

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer <= maintainDistance)
        {
            nmAgent.isStopped = true;
            nmAgent.speed = 0;
        }
        else
        {
            nmAgent.isStopped = false;
            nmAgent.speed = chaseSpeed;
            nmAgent.SetDestination(target.position);
        }

        SummonTimer += Time.deltaTime;
        if (SummonTimer >= SummonTimeInterval)
        {
            ChangeState(State.CAST);
            SummonTimer = 0;
        }

    }

    protected virtual void UpdateCast()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0;

        if (!hasCast)
        { 
            if (currentSummonCount < MaxSummonCount)
            {
                Vector3 randomPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                GameObject enemy = Instantiate(summonedEnemies[Random.Range(0, summonedEnemies.Length)], transform.position + randomPosition, Quaternion.identity);
                enemy.GetComponent<MonsterBase>().summonedMonster = true;
                enemy.GetComponent<MonsterStatus>().SetMaxHealth(20);
                enemy.GetComponent<MonsterBase>().master = GetComponent<Summoner>();
                totalEnemies.Add(enemy);
                currentSummonCount++;
                hasCast = true;
            }
            
        }

        if (Time.time - lastTransitionTime >= 1f)
        {
            ChangeState(State.CHASE);
            hasCast = false; // �÷��� �ʱ�ȭ
        }
    }
    private void OnDestroy()
    {
        foreach (GameObject enemy in totalEnemies)
        {
            if (enemy == null) continue;
            enemy.GetComponent<MonsterBase>().TakeDamage(9999, false);
        }
    }

    public void summonDead(GameObject obj)
    {
      currentSummonCount--;
        totalEnemies.Remove(obj);
    }

}
