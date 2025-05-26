using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FieldMage : MonsterBase
{
    [Header("Field Settings")]
    [SerializeField] TileManager tileManager;
    [SerializeField] protected float maintainDistance = 10f;          // �÷��̾�� ������ �ּ� �Ÿ�
    [SerializeField] protected float fieldSpawnInterval = 5f;     // �ʵ� ���� ����(��)
    protected float fieldSpawnTimer = 0f;                          // Ÿ�̸�

    [SerializeField] private GameObject debuffFieldPrefab;         // �÷��̾� ��ġ�� �� ����� �ʵ�
    [SerializeField] private GameObject buffFieldPrefab;           // ���� ��ġ�� �� ���� �ʵ�

    bool hasCast = false;


    [SerializeField]
    private LayerMask monsterLayer; // ���� Layer
    [SerializeField]
    private LayerMask playerLayer; // �÷��̾� Layer


    protected override void Start()
    {
        base.Start();

        tileManager = FindObjectOfType<TileManager>();
        // stateActions�� CAST ���¿� ���� �޼��� ���
        stateActions[State.CAST] = UpdateCast;
        stateActions.Remove(State.ATTACK);

        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"),LayerMask.NameToLayer("IgnorePlayerCollision"));
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

        fieldSpawnTimer += Time.deltaTime;
        if (fieldSpawnTimer >= fieldSpawnInterval)
        {
            ChangeState(State.CAST);
            fieldSpawnTimer = 0;
        }

    }

    protected virtual void UpdateCast()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0;

        if (!hasCast)
        {
            if (Random.Range(0, 2) == 0) // 0 �Ǵ� 1 ���� (50% Ȯ��)
            {
                PlaceDebuffField();
            }
            else
            {
                PlaceBuffField();
            }

            hasCast = true;
        }

        if (Time.time - lastTransitionTime >= 1f)
        {
            ChangeState(State.CHASE);
            hasCast = false; // �÷��� �ʱ�ȭ
        }
    }
    protected virtual void PlaceDebuffField()
    {
        if (debuffFieldPrefab != null && target != null)
        {
            // �÷��̾� ��ġ�� ����� �ʵ带 ����
            StartCoroutine(BuffFieldRoutine(target.position, debuffFieldPrefab, 0));
        }
    }

    protected virtual void PlaceBuffField()
    {
        if (buffFieldPrefab != null)
        {
            // 1. �ֺ� ���� Ž�� (�ݰ� 10 ����)
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, monsterLayer); // ���� ���̾�

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
                Vector3 spawnPos = lowestHealthMonster.transform.position;
                StartCoroutine(BuffFieldRoutine(spawnPos, buffFieldPrefab, 1));
            }
        }
    }


    private IEnumerator BuffFieldRoutine(Vector3 spawnPos, GameObject field, int mode)
    {

        float fieldSize = 4; // ���� �� �ʵ��� �ݰ�
        float warningDuration = 8.0f; // ���� ǥ�� ���� �ð�

        // 1. ���� ��ǥ�� Ÿ�� �׸��� ��ǥ�� ��ȯ
        int tileX = Mathf.RoundToInt(spawnPos.x / 2);
        int tileZ = Mathf.RoundToInt(spawnPos.z / 2);


        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("TileManager not found in the scene.");
            yield break;
        }

        // 3. �ش� Ÿ���� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        Tile targetTile = tileManager.GetTiles[tileZ, tileX];
        if (targetTile == null || !targetTile.IsSetActive)
        {
            Debug.LogWarning("Target tile is not active");
            yield return null;
        }

        StartCoroutine(tileManager.ShowWarningOnTile(spawnPos, warningDuration, fieldSize, mode));
        yield return new WaitForSeconds(warningDuration);

        //// 4. Ÿ�� ���� �Ҳ� ���� ����
        //Vector3 fireFieldPosition = new Vector3(
        //    tileX * 2,                    // Ÿ���� ���� X ��ǥ
        //    targetTile.transform.position.y + 2.0f,  // Ÿ�� �� �ణ ��
        //    tileZ * 2                     // Ÿ���� ���� Z ��ǥ
        //);

        //Instantiate(field, fireFieldPosition, Quaternion.identity);
    }

}


