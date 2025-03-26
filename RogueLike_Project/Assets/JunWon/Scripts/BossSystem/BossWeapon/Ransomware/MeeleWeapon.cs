using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleWeapon : BaseWeapon
{
    [Header("���� ���� ����")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private bool applyKnockback = true;

    [SerializeField] private float collisionCooldown = 0.5f; // �浹 ��ٿ� �ð� (��)
    private float lastCollisionTime = -1f; // ������ �浹 �ð�
    protected override void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            return; 
        }

        base.OnTriggerEnter(other);

        // �߰����� ���� ���� ȿ�� (��: �˹�)
        if (applyKnockback && isCollisionEnabled && other.CompareTag("Player"))
        {
            if (applyKnockback)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (other.transform.position - transform.position).normalized;
                    direction.y = 0.2f;
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);

                }
            }
        }
    }

    // ���� ���� Ưȭ ��� �߰� ����
}
