using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedMonster : MonoBehaviour
{
    [Header("RangedMonster Stats")]
    [SerializeField] int monsterID_ = 0;
    [SerializeField] float hp_ = 0;
    [SerializeField] float def = 0;


    [Header("Preset Fields")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject splashFx;
    [SerializeField] private NavMeshAgent nmAgent;


    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    [SerializeField] float fireRate = 2f;

    public Weapon gun;
    public Transform firePoint;

    public Transform target;

    private FieldOfView fov;

    private float searchTargetDelay = 0.2f;


    enum State
    {
        IDLE,
        CHASE,
        ATTACK,
        AIMING,
        SHOT,
        KILLED
    }

    State state; // setting situation

    void Start()
    {
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();

        fov = GetComponent<FieldOfView>();

        hp_ = 10;
        state = State.IDLE;
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (hp_ > 0)
        {
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    IEnumerator IDLE()
    {
        // ���� animator �������� ���
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //// �ִϸ��̼� �̸��� IdleNormal �� �ƴϸ� Play
        //if (curAnimStateInfo.IsName("IdleNormal") == false)
        //    anim.Play("IdleNormal", 0, 0);

        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return null;
    }

    IEnumerator CHASE()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        //if (curAnimStateInfo.IsName("WalkFWD") == false)
        //{
        //    anim.Play("WalkFWD", 0, 0);
        //    // SetDestination �� ���� �� frame�� �ѱ������ �ڵ�
        //    yield return null;
        //}


        nmAgent.SetDestination(target.position);

        // ��ǥ������ ���� �Ÿ��� ���ߴ� �������� �۰ų� ������
        if (nmAgent.remainingDistance <= nmAgent.stoppingDistance)
        {
            // StateMachine �� �������� ����
            ChangeState(State.AIMING);
        }

        // ��ǥ ������ ������ ���
        if (fov.visibleTargets.Count == 0 || target == null)
        {
            yield return new WaitForSeconds(searchTargetDelay);
            ChangeState(State.IDLE);
        }
        yield return null;
    }

    IEnumerator ATTACK()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //anim.Play("Attack01", 0, 0);

        // �Ÿ��� �־�����
        if (nmAgent.remainingDistance > nmAgent.stoppingDistance)
        {
            // StateMachine�� �������� ����
            ChangeState(State.CHASE);
        }

        yield return null;
        // ���� animation �� �� �踸ŭ ���
        // �� ��� �ð��� �̿��� ���� ������ ������ �� ����.
        //yield return new WaitForSeconds(curAnimStateInfo.length * 2f);
    }

    IEnumerator AIMING()
    {
        //var curAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
        //anim.Play("Attack01", 0, 0);

        // �Ÿ��� �־�����
        ChangeState(State.SHOT);
        yield return new WaitForSeconds(fireRate);

        // ���� animation �� �� �踸ŭ ���
        // �� ��� �ð��� �̿��� ���� ������ ������ �� ����.
        //yield return new WaitForSeconds(curAnimStateInfo.length * 2f);
    }

    IEnumerator SHOT()
    {
        gun.Fire(transform.rotation);
        ChangeState(State.CHASE);
        yield return null;
    }

    IEnumerator KILLED()
    {
        yield return null;
    }

    void ChangeState(State newState)
    {
        state = newState;
    }

    void Update()
    {
        if (target == null) return;
        // target �� null �� �ƴϸ� target �� ��� ����
        nmAgent.SetDestination(target.position);
    }
}
