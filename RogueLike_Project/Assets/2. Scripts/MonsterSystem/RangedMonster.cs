using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 1.5f;
    protected bool isFired = false;

    public EnemyWeapon gun;

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

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            ChangeState(State.ATTACK);
        }

    }

    protected override void UpdateAttack()
    {
        // Ÿ���� ���ų� ���� ������ ��� ��� ���� ��ȯ
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE); // ���� ���·� ��ȯ
            isFired = false;          // �߻� ���� �ʱ�ȭ
            attackTimer = 0f;         // Ÿ�̸� �ʱ�ȭ
            return;
        }

        nmAgent.isStopped = true; // �̵� ����

        // ���� Ÿ�̸� ����
        attackTimer += Time.deltaTime;

        // ���� �ð� ���� (���� ������ �Ϻθ� ���� �ð����� ���)
        float aimTime = attackCooldown * 0.6f; // ��Ÿ���� 30%�� ���� �ð����� ���
        float attackTime = attackCooldown * 0.8f;
        if (attackTimer <= aimTime)
        {
            // ���� ����
            SetAnimatorState(State.ATTACK); // ATTACK ���¿��� ���� �ִϸ��̼� ����
            return; // ���� �߻����� ����
        }
        else if (attackTimer <= attackTime)
        {
            SetAnimatorState(State.COOLDOWN);
        }
        else
        {
            SetAnimatorState(State.AIM);
        }

        // ���� ����
        //if (!isFired)
        //{
        //    gun.Fire();   // �߻�
        //    isFired = true; // �߻� ���� ����
        //}

        // ���� ��Ÿ�� �Ϸ� �� �ʱ�ȭ
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f; // Ÿ�̸� �ʱ�ȭ
            isFired = false;  // �߻� ���� �ʱ�ȭ
        }
    }

    public void FireEvent()
    {
        if (gun != null)
        {
            gun.Fire(); // �� �߻�
            Debug.Log("Gun fired via Animation Event!");
        }
    }
}
