using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleWeapon : BaseWeapon
{
    [Header("���� ���� ����")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private bool applyKnockback = true;

    protected override void OnTriggerEnter(Collider other)
    {
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
