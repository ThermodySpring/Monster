using System.Collections;
using UnityEngine;

public class FlyingMonster : MonsterBase
{
    [Header("Flying Monster Settings")]
    public float flyHeight = 5.0f; // ���� ����
    public float chaseSpeed = 4.0f; // ���� �ӵ�
    public float attackRange = 2.0f; // ���� ����
    public float attackCooldown = 2.0f; // ���� ����
    public int damage = 15; // ���ݷ�
    public float obstacleAvoidanceDistance = 5.0f; // ��ֹ� ȸ�� �Ÿ�
    public float avoidanceDuration = 1.0f; // ȸ�� ���� �ð�

    private FieldOfView fov; // �þ� ���� ������Ʈ
    private bool canAttack = true; // ���� ���� ����
    private bool isAvoiding = false; // ȸ�� ���� ����
    private Vector3 avoidanceDirection; // ȸ�� ����

    protected override void Start()
    {
        fov = GetComponent<FieldOfView>(); // �þ� ���� ������Ʈ ��������
        target = GameObject.FindGameObjectWithTag("Player").transform; // Ÿ�� ����
        base.Start();
    }

    protected override IEnumerator StateMachine()
    {
        while (hp > 0)
        {
            Debug.Log(state + " state");
            yield return StartCoroutine(state.ToString());
        }
    }

    private IEnumerator IDLE()
    {
        if (fov.visibleTargets.Count > 0)
        {
            target = fov.visibleTargets[0];
            ChangeState(State.CHASE);
        }
        else
        {
            target = null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CHASE()
    {
        while (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Vector3 targetPosition = target.position + Vector3.up * flyHeight;

            if (!isAvoiding)
            {
                // ��ֹ� ������ ���� Raycast
                if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, obstacleAvoidanceDistance))
                {
                    if (hit.collider.CompareTag("Obstacle")) // ��ֹ� �±� Ȯ��
                    {
                        // ��ֹ��� ȸ���ϱ� ���� ���� ����
                        avoidanceDirection = Vector3.Cross(directionToTarget, Vector3.up).normalized;
                        isAvoiding = true;
                        StartCoroutine(AvoidanceCooldown());
                    }
                }
            }

            if (isAvoiding)
            {
                // ȸ�� ���� ����
                transform.position += avoidanceDirection * chaseSpeed * Time.deltaTime;
            }
            else
            {
                // ��ֹ��� ������ ��ǥ �������� �̵�
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
            }

            // ��ǥ�� �ٶ󺸵��� ȸ��
            Vector3 lookDirection = directionToTarget;
            transform.rotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));

            // ���� ������ �����ϸ� ATTACK ���·� ��ȯ
            if (Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                ChangeState(State.ATTACK);
            }

            yield return null; // ���� �����ӱ��� ���
        }
    }

    private IEnumerator AvoidanceCooldown()
    {
        yield return new WaitForSeconds(avoidanceDuration);
        isAvoiding = false; // ȸ�� ���� ����
    }

    private IEnumerator ATTACK()
    {
        if (canAttack)
        {
            canAttack = false;

            // ���� �ִϸ��̼� ���
            anim.SetTrigger("Attack");

            // �÷��̾�� ������ ����
            if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                // Ÿ�ٿ� ������ �ֱ� (�÷��̾�� ������ �޼��� ȣ��)
                // target.GetComponent<PlayerHealth>().TakeDamage(damage);
            }

            yield return new WaitForSeconds(attackCooldown); // ���� ��Ÿ�� ���
            canAttack = true;
        }

        // ���� �� �Ÿ��� �־����� �ٽ� ����
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (hp <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();
        // ��� �ִϸ��̼� ��� �� �߰� ����
    }
}