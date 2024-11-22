using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using static UnityEngine.GraphicsBuffer;


public abstract class MonsterBase : MonoBehaviour
{
    [SerializeField] protected Transform target;
    [SerializeField] private Transform body;        // ĳ���� ��ü (XZ ȸ��)
    [SerializeField] private Transform head;        // �Ӹ� �Ǵ� ��ü (�����¿� ȸ��)
    [SerializeField] private float maxVerticalAngle = 60f; // �Ӹ��� ��/�Ʒ��� ȸ�� ������ �ִ� ����
    protected float rotateSpeed = 2.0f; // ȸ�� �ӵ�



    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;
    [SerializeField] protected FieldOfView fov;
    [SerializeField] private Rigidbody playerRigidBody;


    [Header("NormalStats Fields")]
    [SerializeField] protected MonsterStatus monsterStatus;
    [SerializeField] protected float attackRange = 5.0f; // ���� ����
    [SerializeField] protected float attackCooldown = 3.0f; // ���� ����
    protected float attackTimer = 0.5f; // ���� Ÿ�̸�
    protected float hp = 0; // �⺻ ü��
    protected float dmg = 0; // �⺻ ������
    protected float chaseSpeed; // ���� �ӵ�


    [Header("Delay(CoolTime)")]
    private float lastTransitionTime = 0f;
    private float transitionCooldown = 0.3f;



    [Header("HitVariable")]
    [SerializeField] private float hitCooldown = 1.0f; // �ǰ� ��Ÿ�� (�� ����)
    private float lastHitTime = 0.0f; // ���������� �ǰݵ� �ð�
    private float hitTimer = 0f;
    private float hitDuration = 0.8f; // �ǰ� �ִϸ��̼� ����

    [Header("DieVariable")]
    private float dieTimer = 0f;
    private float dieDuration = 5.0f; // ���� �ִϸ��̼� ����


    protected enum State
    {
        IDLE,
        CHASE,
        ATTACK,
        HIT,
        DIE,
        SEARCH,
        AIM,
        KILL,
        COOLDOWN,
    }


    protected State state;
    protected Coroutine stateMachineCoroutine;
    private Dictionary<State, Action> stateActions;
    private Dictionary<State, float> stateDurations;


    [SerializeField] EnemyCountData enemyCountData;
    bool isDie = false;
    protected virtual void Start()
    {
        stateActions = new Dictionary<State, Action> 
        {

            { State.IDLE, UpdateIdle },
            { State.CHASE, UpdateChase },
            { State.ATTACK, UpdateAttack },
            { State.HIT, UpdateHit },
            { State.DIE, UpdateDie },
        };

        stateDurations = new Dictionary<State, float>
        {
            { State.IDLE, 0.3f },
            { State.CHASE, 0f }, // Ÿ�̸Ӱ� �ʿ� ������ 0���� ����
            { State.ATTACK, 1.0f }, // �ִϸ��̼� ���̿� �°� ����
            { State.HIT, 0.8f }, // Hit �ִϸ��̼� ����
            { State.DIE, 5.0f }, // ���� �ִϸ��̼� ����
        };

        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        monsterStatus = GetComponent<MonsterStatus>();
        fov = GetComponent<FieldOfView>();


        hp = monsterStatus.GetHealth(); // �⺻ ü��
        dmg = monsterStatus.GetAttackDamage(); // �⺻ ���ݷ�
        chaseSpeed = monsterStatus.GetMovementSpeed(); // �⺻ �̵� �ӵ�
        // attackRange = monsterStatus.GetAttackRange(); // �⺻ ���� ����


        state = State.IDLE;
    } // �⺻ ���� ����

  

    #region animationsettings
    protected void SetAnimatorState(State state)
    {
        if (anim != null)
        {
            if (state == State.HIT)
            {
                anim.Play("GetHit", 0, 0f); // Ʈ���Ÿ� ����� �ִϸ��̼� ���� ���
            }
            else
            {
                anim.SetInteger("State", (int)state); // �ٸ� ���´� Integer�� ó��
            }
        }
    }
    protected float GetAnimationClipLength(string clipname)
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == clipname)
                {
                    return clip.length;
                }
            }
        }
        // �⺻�� ���� (�ִϸ��̼� Ŭ���� ã�� ���� ���)
        return 1.0f; // �ʿ信 ���� ����
    }
    #endregion


    //protected IEnumerator Crowd_Control(Transform target)
    //{
    //    target.GetComponent<PlayerControl>().enabled = false;
    //    yield return new WaitForSeconds(0.5f);
    //    target.GetComponent <PlayerControl>().enabled = true;
    //}


    protected virtual void Update()
    {
        Debug.Log(name + " current state = " + state);
        if (state == State.IDLE) CheckPlayer();
        if (state == State.CHASE || state == State.ATTACK)
        {
            RotateTowardsTarget();
        }
        PlayAction(state);
    }

    protected virtual void LateUpdate()
    {
        if (state == State.CHASE || state == State.ATTACK)
        {
            RotateTowardsTarget();
        }
    }

    private void PlayAction(State state)
    {
        if (stateActions.TryGetValue(state, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Debug.LogWarning($"State {state}�� ���� �׼��� ���ǵ��� �ʾҽ��ϴ�.");
        }
    }


    // �׻� �������� ���
    private void CheckPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            if (state != State.ATTACK || state != State.HIT) ChangeState(State.CHASE);
        }
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        // Ÿ�� ���� ���
        Vector3 direction = (target.position - transform.position).normalized;

        // ���� ���͸� �������� ȸ�� ���
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // �ε巴�� ȸ��
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);

    }
    protected virtual void UpdateIdle()
    {
    }

    protected virtual void UpdateChase()
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

    protected virtual void UpdateAttack()
    {

        nmAgent.isStopped = true;
        nmAgent.speed = 0f;

        // ���� Ÿ�̸� ����
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown) 
        {
            // ���� �� Ÿ���� ������ ����ٸ� ���� ���·� ��ȯ
            if (Vector3.Distance(transform.position, target.position) > attackRange)
            {
                ChangeState(State.CHASE);
                return;
            }

            // ���� Ÿ�̸� �ʱ�ȭ
            attackTimer = 0f;
            ChangeState(State.ATTACK);
        }
    }
     

    protected virtual void UpdateHit()
    {
        nmAgent.isStopped = true;

        hitTimer += Time.deltaTime;
        if (hitTimer >= hitDuration)
        {
            ChangeState(State.CHASE);
            hitTimer = 0f;
        }
    }


   
    protected void UpdateDie()
    {
        nmAgent.isStopped = true;

        dieTimer += Time.deltaTime;
        if (dieTimer >= dieDuration)
        {
            // ������Ʈ �ı� �Ǵ� ��Ȱ��ȭ
            Destroy(gameObject);
        }
    }

    protected void ChangeState(State newState)
    {

        if (Time.time - lastTransitionTime < transitionCooldown)
            return;

        lastTransitionTime = Time.time;

        if (state != newState || newState == State.HIT || newState == State.ATTACK)
        {
            Debug.Log(transform.name + " ���� ����: " + state + " �� " + newState);
            SetAnimatorState(newState);
            state = newState;

            // ���º� �ʱ�ȭ
            switch (state)
            {
                case State.ATTACK:
                    attackTimer = 0f;
                    break;
                case State.HIT:
                    hitTimer = 0f;
                    break;
                case State.DIE:
                    dieTimer = 0f;
                    break;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        // ü�� ���� ó��
        monsterStatus.DecreaseHealth(damage);
        hp = monsterStatus.GetHealth();

        if (hp > 0)
        {
            // �ǰ� ���·� ��ȯ
            ChangeState(State.HIT);
            target = FindObjectOfType<PlayerStatus>().transform;
        }
        else
        {
            // ���� ó��
            isDie = true; // ���� �÷��� ����
            ChangeState(State.DIE);
            dieTimer = 0.0f;
        }
    }

}
