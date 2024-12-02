using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HammerMan : MonsterBase
{
    [Header("HammerMan Settings")]
    [SerializeField] private float jumpForce = 15f; // ���� ��
    [SerializeField] private float jumpCooldown = 2f; // ���� ��Ÿ��
    [SerializeField] private float shockwaveRadius = 5f; // ����� �ݰ�
    [SerializeField] private float shockwaveDamage = 20f; // ����� ������
    [SerializeField] private LayerMask groundLayer; // ����� �������� ���� ���̾�

    [SerializeField]  private NavMeshPath navPath; // NavMesh ���
    private Rigidbody rb; // Rigidbody
    [SerializeField] private bool isJumping = false; // ���� ���� Ȯ��
    [SerializeField] private bool canJump = true; // ���� ���� ����

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        navPath = new NavMeshPath();
    }

    protected override void UpdateChase()
    {
        if (target == null)
        {
            ChangeState(State.IDLE);
            return;
        }

        if (!isJumping && canJump)
        {
            JumpTowardsNextPoint();
        }
    }

    protected override void UpdateAttack()
    {
        if (target == null || Vector3.Distance(transform.position, target.position) > attackRange)
        {
            ChangeState(State.CHASE);
            return;
        }

        if (!isJumping && canJump)
        {
            JumpTowardsNextPoint();
        }
    }

    private void JumpTowardsNextPoint()
    {
        if (!NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navPath))
        {
            Debug.LogError("Failed to calculate path to target.");
            return;
        }

        if (navPath.corners.Length > 1)
        {
            // ���� ���� ���� ����
            Vector3 nextJumpPoint = navPath.corners[1];
            Vector3 jumpDirection = (nextJumpPoint - transform.position).normalized;

            // Rigidbody ����
            rb.AddForce(new Vector3(jumpDirection.x, 1, jumpDirection.z) * jumpForce, ForceMode.Impulse);
            isJumping = true;
            canJump = false;

            StartCoroutine(JumpCooldown());
        }
        else
        {
            Debug.LogWarning("No valid path corners found.");
        }
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isJumping && collision.collider.CompareTag("Floor"))
        {
            isJumping = false;

            // ����� ����
            CreateShockWave();

            // ���� ��ȯ
            if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                ChangeState(State.ATTACK);
            }
            else
            {
                ChangeState(State.CHASE);
            }
        }
    }

    private void CreateShockWave()
    {
        Collider[] hitTargets = Physics.OverlapSphere(transform.position, shockwaveRadius, groundLayer);
        foreach (var hit in hitTargets)
        {
            PlayerStatus player = hit.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(shockwaveDamage);
            }
        }

        Debug.Log("Shockwave created!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);

        if (navPath != null && navPath.corners.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < navPath.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(navPath.corners[i], navPath.corners[i + 1]);
            }
        }
    }
}
