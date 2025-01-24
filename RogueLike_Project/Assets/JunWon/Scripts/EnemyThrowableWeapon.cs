using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [SerializeField] private GameObject fieldPrefab; // ������ ���� ������
    [SerializeField] private float explosionRadius = 3f; // ���� ����
    [SerializeField] private LayerMask targetLayer; // �浹 ��� ���̾�
    [SerializeField] private float fireFieldDuration = 5f; // ���� �ð�

    private bool hasExploded = false;


    private void OnCollisionEnter(Collision collision)
    {
        // �ٴڰ� �浹 Ȯ��
        if (collision.gameObject.CompareTag("Floor"))
        {
            if (hasExploded) return;
            hasExploded = true;
            if (fieldPrefab == null)
            {
                Debug.Log("Field prefab is missing. Please assign it.");
            }

            //Debug.Log($"Number of contacts: {collision.contacts.Length}");
            //foreach (var contact in collision.contacts)
            //{
            //    Debug.Log($"Contact point: {contact.point}");
            //}

            Debug.Log("Boom");
            // ��ź ����
            Destroy(gameObject);

            // ��Ÿ������ �ʵ� ����
            GameObject fireField = Instantiate(fieldPrefab,
                                               collision.contacts[0].point,
                                               Quaternion.identity);

            // �ʵ� ���� �ð� ����
            Destroy(fireField, fireFieldDuration);
        }
    }
    
}
