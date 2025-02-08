using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Phase1_BasicRangedAttack_State : State<Ransomware>
{
    public Phase1_BasicRangedAttack_State(Ransomware owner) : base(owner) { }

    Vector3 playerPos;
    Vector3 firePos;
    Quaternion fireRot;
    GameObject packet;
    public override void Enter()
    {
        playerPos = owner.Player.transform.position;
        firePos = owner.FirePoint.transform.position;
        fireRot = owner.FirePoint.transform.rotation;
        packet = owner.DataPacket;
        Debug.Log("[Phase1_BasicRangedAttack_State] Enter");
        owner.NmAgent.isStopped = true;

        if (owner.AbilityManger.UseAbility("BasicRangedAttack"))
        {
            FireProjectile();
        }
    }

    public override void Update()
    {

    }

    void FireProjectile()
    {
        if (playerPos != null && packet != null && firePos != null)
        {
            Vector3 directionToPlayer = (playerPos - firePos).normalized;
            GameObject projectile = GameObject.Instantiate(owner.DataPacket, firePos, fireRot);
            projectile.GetComponent<MProjectile>().SetBulletDamage(owner.AbilityManger.GetAbiltiyDmg("BasicRangedAttack")); // ���� �����Ϳ��� ������ �� ���������� ���� (����)
            projectile.GetComponent<MProjectile>().SetDirection(directionToPlayer);
            Debug.Log("���Ÿ� ��ü �߻�!");
        }   
        else
        {
            Debug.LogWarning("��ü �߻翡 �ʿ��� ������ �Ǵ� �߻� ������ �������� ����.");
        }
    }

    public bool IsAnimationFinished()
    {
        // ����: �ִϸ������� ���� �ִϸ��̼� ���°� "Attack" �ִϸ��̼��� �ƴϰ�, ��ȯ ���� �ƴ� ��
        return true;
        //return !owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialAttack") && !owner.Animator.IsInTransition(0);
    }
}
