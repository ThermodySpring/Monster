using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Phase2_DataBlink_State : State<Ransomware>
{
    private bool isAttackFinished = false;
    private readonly float teleportDistance = 6f;
    private Vector3 targetPosition;

    public Phase2_DataBlink_State(Ransomware owner) : base(owner)
    {
        owner.SetDataBlinkState(this);
    }

    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_DataBlink_State] Enter");
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            // �ڷ���Ʈ�� ��ġ �̸� ���
            targetPosition = CalculateTargetPosition();
            owner.Animator.SetTrigger("DataBlink");
            if (owner.AbilityManager.UseAbility("DataBlink"))
            {
                // �ִϸ��̼��� �˾Ƽ� �����
            }
        }
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("DataBlink");
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnStartDisappear()
    {
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnShowWarning()
    {
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnTeleport()
    {
        Debug.Log("Go Teleport");
        owner.transform.position = targetPosition;
    }

    private Vector3 CalculateTargetPosition()
    {
        if (owner.Player == null) return owner.transform.position;

        float targetAngle = Random.Range(-60f, 60f);
        Vector3 directionToPlayer = (owner.Player.position - owner.transform.position).normalized;
        Vector3 targetDirection = Quaternion.Euler(0, targetAngle, 0) * directionToPlayer;
        Vector3 position = owner.Player.position + (targetDirection * teleportDistance);

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return owner.transform.position;
    }



}
