using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenadier : RangedMonster
{
    [Header("Grenadier Settings")]
    [SerializeField] private Transform throwPoint; // ��ô ���� ��ġ
    [SerializeField] private GameObject throwablePrefab; // ��ôü ������
    [SerializeField] private float throwForce = 5f; // ��ô ��
    [SerializeField] private float arcHeight = 3f; // �������� �ְ��� ����

    protected override void Start()
    {
        base.Start();
        aimTime = attackCooldown * 0.2f;
        aimTime = attackCooldown * 0.4f;
    }
    public override void FireEvent()
    {
        if (throwablePrefab != null && target != null)
        {
            Vector3 targetPosition = target.position;

            // ��ô ���� ���
            Throw(targetPosition);
            //Debug.Log($"Grenade thrown at {targetPosition}");
        }
    }

    private void Throw(Vector3 targetPosition)
    {
        // ��ôü ����
        GameObject throwable = Instantiate(throwablePrefab, throwPoint.position, Quaternion.identity);

        // ��ôü ���� ���
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = CalculateThrowDirection(throwPoint.position, targetPosition, arcHeight);
            rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
    }

    private Vector3 CalculateThrowDirection(Vector3 start, Vector3 target, float height)
    {
        // ������ ���� ���
        Vector3 direction = target - start;
        direction.y = 0; // ���� ����
        float distance = direction.magnitude;
        direction.Normalize();

        float verticalVelocity = Mathf.Sqrt(2 * Physics.gravity.magnitude * height);
        float time = Mathf.Sqrt(2 * height / Physics.gravity.magnitude) + Mathf.Sqrt(2 * (height + target.y - start.y) / Physics.gravity.magnitude);

        Vector3 throwDirection = direction * (distance / time) + Vector3.up * verticalVelocity;
        return throwDirection;
    }
}
