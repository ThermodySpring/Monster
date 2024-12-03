using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [SerializeField] private GameObject fieldPrefab; // ������ ���� ������
    [SerializeField] private float explosionRadius = 3f; // ���� ����
    [SerializeField] private LayerMask targetLayer; // �浹 ��� ���̾�

    private void OnCollisionEnter(Collision collision)
    {
        Explode(collision.contacts[0].point);
    }
    private void Explode(Vector3 position)
    {
        // ���� ��ġ�� ���� ����
        if (fieldPrefab != null)
        {
            Instantiate(fieldPrefab, position, Quaternion.identity);
        }

        // ���� �� ���� �� ������ �ʱ� ������ ���� (����)
        Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius, targetLayer);
        foreach (var collider in hitColliders)
        {
            PlayerStatus player = collider.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(20f); // ���� �ʱ� ������
            }
        }

        Destroy(gameObject, 5.0f); // ��ôü ����
    }
}
