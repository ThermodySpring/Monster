using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MProjectile : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 20; // ����ü�� ���ط�
    [SerializeField] float lifetime = 5f; // ����ü�� ����
    [SerializeField] float speed = 0.05f; // ����ü�� �ӵ�

    void Start()
    {
        Destroy(gameObject, lifetime); // ���� �ð� �� ����ü �ı�
    }

    void Update()
    {
        UpdateBullet();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player takes damage");
            PlayerControl playerHealth = other.GetComponent<PlayerControl>();
            if (playerHealth != null)
            {
                // playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }

    void UpdateBullet()
    {
        transform.Translate(Vector3.forward * speed);
    }
}