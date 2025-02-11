using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Phase1_SpeacialAttack_State : State<Ransomware>
{
    private bool isAttackFinished = false;
    public float radius = 15.0f;
    public float damage = 50f;
    public LayerMask playerLayer; // Inspector���� Player ���̾ ����

    public Phase1_SpeacialAttack_State(Ransomware owner) : base(owner) {
        owner.SetSpecialAttackState(this);
        playerLayer = LayerMask.GetMask("Character");
    }


    public override void Enter()
    {
        isAttackFinished = false;
        owner.NmAgent.isStopped = true;

        if (CanExecuteAttack())
        {
            // Ability �ý����� ���� ���� ����
            if (owner.AbilityManger.UseAbility("DataExplode"))
            {
                owner.Animator.SetTrigger("DataExplode");
                LockPlayerSkill();
                ExplodeData();
            }
        }
        else
        {
            Debug.LogWarning("Cannot execute attack - missing components");
            isAttackFinished = true; // ���� �Ұ����� ��� �ٷ� ���� ��ȯ
        }
    }

    public override void Update()
    {
     
    }

    public override void Exit()
    {
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("DataExplode");

    }

   

    public void ExplodeData()
    {
        // Player ���̾ �ִ� ������Ʈ�� ����
        Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius, playerLayer);
        Debug.Log("Player ���� " + hits.Length);

        foreach (Collider hit in hits)
        {
            PlayerStatus playerHealth = hit.GetComponent<PlayerStatus>();
            if (playerHealth != null)
            {
                // �Ÿ��� ���� ������ ���
                float distance = Vector3.Distance(owner.transform.position, hit.transform.position);
                float damageAmount = damage * (1 - (distance / radius));

                playerHealth.DecreaseHealth(damageAmount);
            }
        }

        // �ð��� ȿ���� ��� ������Ʈ�� ���� ����
        ShowExplosionEffect();
    }

    void ShowExplosionEffect()
    {
        // ���⿡ ��ƼŬ ȿ��, ���� �� �߰�
    }

    void LockPlayerSkill()
    {
        Character player = owner.Player.GetComponent<Character>();
        if (player != null)
        {
            Debug.Log("IsWeaponExchangeLocked");
            player.IsCursorLocked();
            player.LockChangedWeapon();
        }
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
}
