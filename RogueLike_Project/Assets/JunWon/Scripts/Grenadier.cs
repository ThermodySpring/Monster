using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenadier : RangedMonster
{
    [Header("Grenadier Settings")]
    [SerializeField] private Transform throwPoint; // ��ô ���� ��ġ
    [SerializeField] private GameObject throwablePrefab; // ��ôü ������
    [SerializeField] private float throwForce = 15f; // ��ô �� (����)
    [SerializeField] private float arcHeight = 5f; // �������� �ְ��� ����
    [SerializeField] private float maxThrowDistance = 20f; // �ִ� ��ô �Ÿ�
    [SerializeField] private bool useHighArc = true; // ���� �˵� vs ���� �˵�

    [Header("Prediction Settings")]
    [SerializeField] private bool predictPlayerMovement = true; // �÷��̾� ������ ����
    [SerializeField] private float predictionTime = 0.5f; // ���� �ð�
    [SerializeField] private LayerMask obstacleLayer = -1; // ��ֹ� ���̾�

    [Header("Debug")]
    [SerializeField] private bool showTrajectory = true; // ���� �ð�ȭ
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.3f; // ���� �ð�
        attackTime = attackCooldown * 0.7f; // ���� �ð�

        // ���� �ð�ȭ ������Ʈ ����
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
        }

        if (trajectoryLine != null && showTrajectory)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.enabled = false;
        }
    }

    public override void FireEvent()
    {
        if (throwablePrefab != null && target != null)
        {
            Vector3 targetPosition = GetPredictedTargetPosition();

            // ��ô ������ �Ÿ����� Ȯ��
            float distance = Vector3.Distance(throwPoint.position, targetPosition);
            if (distance > maxThrowDistance)
            {
                // �ִ� �Ÿ��� ����
                Vector3 direction = (targetPosition - throwPoint.position).normalized;
                targetPosition = throwPoint.position + direction * maxThrowDistance;
            }

            // ���� �ð�ȭ �����
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }

            // ��ô ���� ���
            ThrowGrenade(targetPosition);
        }
    }

    // �÷��̾� �������� ������ Ÿ�� ��ġ ���
    private Vector3 GetPredictedTargetPosition()
    {
        if (!predictPlayerMovement || target == null)
            return target.position;

        // �÷��̾��� ���� �ӵ� ��������
        Rigidbody playerRb = target.GetComponent<Rigidbody>();
        Vector3 playerVelocity = Vector3.zero;

        if (playerRb != null)
        {
            playerVelocity = playerRb.velocity;
        }
        else
        {
            // Rigidbody�� ���ٸ� CharacterController Ȯ��
            CharacterController playerCC = target.GetComponent<CharacterController>();
            if (playerCC != null)
            {
                // CharacterController�� ��� ���� �ӵ��� ���ϱ� �����Ƿ� �ٻ�ġ ���
                playerVelocity = (target.position - target.position) / Time.deltaTime;
            }
        }

        // ������ ��ġ ���
        Vector3 predictedPosition = target.position + playerVelocity * predictionTime;

        // ���鿡 ���߱� (Y�� ����)
        RaycastHit hit;
        if (Physics.Raycast(predictedPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            predictedPosition.y = hit.point.y;
        }

        return predictedPosition;
    }

    private void ThrowGrenade(Vector3 targetPosition)
    {
        // �ּ� ���� ���� (�ʹ� ������ ������ ��� �߹ؿ� ������)
        if (targetPosition.y < throwPoint.position.y)
        {
            targetPosition.y = throwPoint.position.y;
        }

        // ��ôü ����
        GameObject grenade = Instantiate(throwablePrefab, throwPoint.position, Quaternion.identity);

        // ��ôü�� ������ ���� (�ִٸ�)
        EnemyThrowableWeapon explosive = grenade.GetComponent<EnemyThrowableWeapon>();
        if (explosive != null)
        {
            explosive.SetExplosionDamage(monsterStatus.GetAttackDamage());
        }

        // ��ôü ���� ��� �� ����
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // ������ ������ ��� ��� ���
            Vector3 throwVelocity = CalculateThrowVelocity(throwPoint.position, targetPosition);
            rb.velocity = throwVelocity;

            // ȸ�� ȿ�� �߰� (�� �ڿ������� ������)
            rb.angularVelocity = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(-3f, 3f),
                Random.Range(-3f, 3f)
            );

            Debug.Log($"Grenade thrown with velocity: {throwVelocity}, magnitude: {throwVelocity.magnitude}");
        }
    }

    private Vector3 CalculateThrowVelocity(Vector3 startPos, Vector3 targetPos)
    {
        Vector3 displacement = targetPos - startPos;
        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0, displacement.z);
        float horizontalDistance = horizontalDisplacement.magnitude;
        float verticalDistance = displacement.y;

        // ������ �ּ�/�ִ� ���� ����
        float throwAngle = useHighArc ? 60f : 30f; // �� ����
        throwAngle = Mathf.Clamp(throwAngle, 15f, 75f); // 15~75�� ���̷� ����

        float angleRad = throwAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y); // �߷��� �׻� �����

        // �ʿ��� �ʱ� �ӵ� ���
        float velocityMagnitude = CalculateRequiredVelocity(horizontalDistance, verticalDistance, angleRad, gravity);

        // ���� ���� ���
        Vector3 horizontalDirection = horizontalDisplacement.normalized;

        // �ӵ� ���� ����
        Vector3 velocity = horizontalDirection * velocityMagnitude * Mathf.Cos(angleRad) +
                          Vector3.up * velocityMagnitude * Mathf.Sin(angleRad);

        Debug.Log($"Throwing at angle: {throwAngle}��, velocity magnitude: {velocityMagnitude}, horizontal distance: {horizontalDistance}");

        return velocity;
    }

    private float CalculateRequiredVelocity(float horizontalDist, float verticalDist, float angle, float gravity)
    {
        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);
        float tanAngle = Mathf.Tan(angle);

        // v�� = (g * x��) / (2 * cos��(��) * (x * tan(��) - y))
        float denominator = 2f * cosAngle * cosAngle * (horizontalDist * tanAngle - verticalDist);

        if (denominator <= 0)
        {
            // ����� �Ұ����� ��� �⺻�� ���
            Debug.LogWarning("Invalid trajectory calculation, using default throw force");
            return throwForce;
        }

        float velocitySquared = (gravity * horizontalDist * horizontalDist) / denominator;

        if (velocitySquared <= 0)
        {
            return throwForce;
        }

        float calculatedVelocity = Mathf.Sqrt(velocitySquared);

        // �ӵ� ���� (�ʹ� �����ų� ������ �ʰ�)
        return Mathf.Clamp(calculatedVelocity, 5f, 30f);
    }


    // Ư�� �ð������� ��ġ ���
    private Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 initialVelocity, float time)
    {
        Vector3 gravity = Physics.gravity;
        return startPos + initialVelocity * time + 0.5f * gravity * time * time;
    }

    // ���� �� ���� �̸����� (����׿�)
    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        if (showTrajectory && trajectoryLine != null && target != null)
        {
            if (attackTimer > aimTime * 0.5f && attackTimer < attackTime)
            {
                ShowTrajectoryPreview();
            }
        }
    }

    private void ShowTrajectoryPreview()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        Vector3 targetPos = GetPredictedTargetPosition();
        Vector3 velocity = CalculateThrowVelocity(throwPoint.position, targetPos);

        Vector3[] points = new Vector3[trajectoryPoints];
        float maxTime = 3f; // �ִ� 3�ʱ��� ���� ǥ��

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = (float)i / (trajectoryPoints - 1) * maxTime;
            points[i] = CalculatePositionAtTime(throwPoint.position, velocity, t);

            // ���鿡 ������ ���� ����
            if (points[i].y <= targetPos.y)
            {
                points[i].y = targetPos.y;
                // ������ ������ ������ ������ ����
                for (int j = i + 1; j < trajectoryPoints; j++)
                {
                    points[j] = points[i];
                }
                break;
            }
        }

        trajectoryLine.SetPositions(points);
    }

    // ����� �ð�ȭ
    private void OnDrawGizmosSelected()
    {
        if (throwPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(throwPoint.position, 0.2f);

            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(throwPoint.position, target.position);

                // �ִ� ��ô �Ÿ� ǥ��
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(throwPoint.position, maxThrowDistance);
            }
        }
    }
}
