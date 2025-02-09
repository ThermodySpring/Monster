using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    private bool isAttackFinished = false;

    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner) {
        owner.SetRangedAttackState(this);
    }

    public override void Enter()
    {
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            // �ִϸ��̼� ���
            owner.Animator.SetTrigger("RangedAttack");

            // Ability �ý����� ���� ���� ����
            if (owner.AbilityManger.UseAbility("BasicRangedAttack"))
            {
                FireProjectile();
            }
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // ���� �Ұ����� ��� �ٷ� ���� ��ȯ
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null &&
               owner.DataPacket != null &&
               owner.FirePoint != null;
    }

    private void FireProjectile()
    {
        Vector3 firePos = owner.FirePoint.position;
        Vector3 directionToPlayer = (owner.Player.position - firePos).normalized;

        GameObject projectile = GameObject.Instantiate(
            owner.DataPacket,
            firePos,
            Quaternion.LookRotation(directionToPlayer)
        );

        if (projectile.TryGetComponent<MProjectile>(out var mProjectile))
        {
            mProjectile.SetBulletDamage(owner.AbilityManger.GetAbiltiyDmg("BasicRangedAttack"));
            mProjectile.SetDirection(directionToPlayer);
            Debug.Log("Ranged attack projectile fired!");
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �޼���
    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RangedAttack"); 
        isAttackFinished = false;
    }

    public bool IsAnimationFinished() => isAttackFinished;
}
