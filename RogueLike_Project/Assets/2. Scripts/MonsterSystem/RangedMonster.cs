using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.VirtualTexturing;

public class RangedMonster : MonsterBase
{
    [Header("settings")]
    [SerializeField] float firerate = 1.5f;
    [SerializeField] protected bool isHitScan = false;

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

        // ���� ��Ÿ�� �Ϸ� �� �ʱ�ȭ
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f; // Ÿ�̸� �ʱ�ȭ
        }
    }

    public void FireEvent()
    {
        if (gun == null)
        {
            return;
        }

        if (!isHitScan)
        {
            gun.Fire(); // �� �߻�
            Debug.Log("Gun fired via Animation Event!");
        }
        else
        {
            gun.FireLaser();
            Debug.Log("Hit scan Activated");
        }

        
    }
}
