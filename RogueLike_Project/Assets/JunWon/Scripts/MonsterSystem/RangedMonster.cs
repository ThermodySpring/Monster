using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 1.5f;
    bool isFired = false;

    public EnemyWeapon gun;

    protected override void UpdateAttack()
    {
        nmAgent.isStopped = true;
        nmAgent.speed = 0f;

        // ���� Ÿ�̸� ����
        if (!isFired)
        {
            gun.Fire();
            isFired = true;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            // ���� �� Ÿ���� ������ ����ٸ� ���� ���·� ��ȯ
            if (Vector3.Distance(transform.position, target.position) > attackRange)
                ChangeState(State.CHASE);

            // ���� Ÿ�̸� �ʱ�ȭ
            attackTimer = 0f;
            isFired = false;
        }
      
    }


  
}
