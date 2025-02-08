using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1State_Ransomware : BossPhaseBase<Ransomware>
{
    private enum Phase1SubState
    {
        Idle,
        Approach,
        Attack,
        Special
    }

    // ���� FSM
    private StateMachine<Ransomware> subFsm;

    // ���� ����


    public Phase1State_Ransomware(Ransomware owner) : base(owner)
    {
    }

    float attackRange = 5.0f;
    float rangedRange = 20.0f;
    float specialAttackChance = 0.2f; // 20% Ȯ��
    float basicRangedAttackChance = 0.4f; // 40% Ȯ��
    float basicMeeleAttackChance = 0.4f; // 40% Ȯ��


    private void InitializeStats()
    {
        owner.MonsterStatus.SetMovementSpeed(5.0f);
    }


    private void InitializeSubFSM()
    {
        owner.AbilityManger.SetAbilityActive("BasicRangedAttack");
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
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange 
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            rangedAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= rangedRange
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            chaseState,
            meleeAttackState,
            () => Vector3.Distance(owner.transform.position, owner.Player.position) <= attackRange 
        ));

        subFsm.AddTransition(new Transition<Ransomware>(
            specialAttackState,
            chaseState,
            () => true
        ));
        subFsm.AddTransition(new Transition<Ransomware>(
            meleeAttackState,
            chaseState,
            () => true
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
        InitializeSubFSM();
    }

    public override void Update()
    {
        subFsm.Update();
    }
}
