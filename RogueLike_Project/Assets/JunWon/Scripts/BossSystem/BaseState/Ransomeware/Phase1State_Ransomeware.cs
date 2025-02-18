using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1State_Ransomware : BossPhaseBase<Ransomware>
{
    private StateMachine<Ransomware> subFsm;

    public Phase1State_Ransomware(Ransomware owner) : base(owner)
    {
    }

    float attackRange = 5.0f;
    float rangedRange = 20.0f;
    float specialAttackChance = 0.2f; // 20% Ȯ��
    float basicRangedAttackChance = 0.4f; // 40% Ȯ��
    float basicMeeleAttackChance = 0.4f; // 40% Ȯ��


    private void InitializeAbility()
    {
        owner.AbilityManager.SetAbilityActive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityActive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityActive("DataExplode");
        owner.AbilityManager.SetMaxCoolTime("DataExplode");

    }
    private void InitializeStats()
    {
        owner.MonsterStatus.SetMovementSpeed(5.0f);
    }


    private void InitializeSubFSM()
    {
       
        // �� ���� �ʱ�ȭ (�� ���� Ŭ������ �����ڿ��� owner�� �޽��ϴ�)
        var idleState = new Phase1_Idle_State(owner);
        var chaseState = new Phase1_Chase_State(owner);
        var meleeAttackState = new Phase1_Attack_State(owner);                // ���� ����
        var rangedAttackState = new Phase1_BasicRangedAttack_State(owner);      // ���Ÿ� ����
        var specialAttackState = new Phase1_SpeacialAttack_State(owner);        // Ư�� ����

        subFsm = new StateMachine<Ransomware>(idleState);

        subFsm.AddTransition(new Transition<Ransomware>(
            idleState,
            chaseState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) > attackRange
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            specialAttackState,
            () => owner.AbilityManager.GetAbilityRemainingCooldown("DataExplode") == 0
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= rangedRange &&
            owner.AbilityManager.GetAbilityRemainingCooldown("BasicRangedAttack") == 0
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            meleeAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange &&
            owner.AbilityManager.GetAbilityRemainingCooldown("BasicMeeleAttack") == 0
        ));


        subFsm.AddTransition(new Transition<Ransomware>(
            specialAttackState,
            chaseState,
            () => specialAttackState.IsAnimationFinished()
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            meleeAttackState,
            chaseState,
            () => meleeAttackState.IsAnimationFinished()
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            rangedAttackState,
            chaseState,
            () => rangedAttackState.IsAnimationFinished()
        ));
    }



    public override void Enter()
    {
        Debug.Log("�������� ���� ������1 ����");
        InitializeStats();
        InitializeAbility();
        InitializeSubFSM();
    }

    public override void Update()
    {
        if (isInterrupted) return;
        subFsm.Update();
    }

    public override void Exit()
    {
        // ������ ���� �� ����FSM�� ����
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Exit();
        }
        subFsm = null;
    }

    public override void Interrupt()
    {
        if (isInterrupted) return;
        isInterrupted = true;

        // ���� ���� ���� ���� ������Ʈ�� Interrupt ȣ��
        if (subFsm != null && subFsm.CurrentState != null)
        {
            subFsm.CurrentState.Interrupt();
        }

        // ������1 ���� ���� �۾�
        owner.AbilityManager.SetAbilityInactive("BasicMeeleAttack");
        owner.AbilityManager.SetAbilityInactive("BasicRangedAttack");
        owner.AbilityManager.SetAbilityInactive("DataExplode");

        owner.NmAgent.isStopped = true;
        owner.SetRotationLock(true);
    }

}
