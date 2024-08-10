using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class MonsterBase : MonoBehaviour, ICombatant
{
    [Header("NormalStats Fields")]
    [SerializeField] protected float hp;
    [SerializeField] protected float def;
    [SerializeField] protected Transform target;

    [Header("Preset Fields")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected GameObject splashFx;
    [SerializeField] protected NavMeshAgent nmAgent;


    [Header("Delay(CoolTime)")]
    [SerializeField] protected float transitionDelay;

    protected Coroutine stateMachineCoroutine;

    protected enum State
    {
        IDLE,
        ATTACK,
        CHASE,
        AIMING,
        SHOT,
        KILL,
    }

    protected State state;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();

        hp = 10; // �⺻ ü��
        state = State.IDLE;
        stateMachineCoroutine = StartCoroutine(StateMachine());
    }

    protected abstract IEnumerator StateMachine();

    public virtual void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp > 0)
        {
            ChangeState(State.CHASE);
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        else
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        // ���� ����ϸ� ������ ���� (��: �ִϸ��̼� ���, ������Ʈ ��Ȱ��ȭ ��)
        Destroy(gameObject);
    }

    protected void ChangeState(State newState)
    {
        state = newState;
    }
}
