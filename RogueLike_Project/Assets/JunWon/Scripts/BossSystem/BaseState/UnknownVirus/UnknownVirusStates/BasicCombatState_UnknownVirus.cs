using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCombatState_UnknownVirus : State<UnknownVirusBoss>
{
    public BasicCombatState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        // NavMeshAgent Ȱ��ȭ �� �̵� �ӵ� ����
        owner.NmAgent.isStopped = false;
        owner.NmAgent.speed = owner.BossStatus.GetMovementSpeed();
        // �̵� �ִϸ��̼� ����
        owner.Animator.SetBool("IsMoving", true);
    }

    public override void Update()
    {
        // �÷��̾ ���� �̵�
        if (owner.NmAgent.isOnNavMesh && owner.Player != null)
        {
            owner.NmAgent.SetDestination(owner.Player.position);

            // ���� �ӵ��� ���� �ִϸ��̼� �Ķ���� ����
            float currentSpeed = owner.NmAgent.velocity.magnitude;
            owner.Animator.SetFloat("MoveSpeed", currentSpeed);
        }

        // �⺻ ����, �� ����, �� ���� Ÿ�̸� ������
        // UnknownVirusBoss.Update() ���ο��� fsm.CurrentState == basicCombatState �� �� �ڵ� ȣ��˴ϴ�.
    }

    public override void Exit()
    {
        // �̵� �ִϸ��̼� ����
        owner.Animator.SetBool("IsMoving", false);
        owner.Animator.SetFloat("MoveSpeed", 0f);
        // NavMeshAgent ����
        owner.NmAgent.isStopped = true;
    }
}
