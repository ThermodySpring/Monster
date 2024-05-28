using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Settings")]
    public int damage = 20; // ����ü�� ���ط�
    public float lifetime = 5f; // ����ü�� ����

    void Start()
    {
        Destroy(gameObject, lifetime); // ���� �ð� �� ����ü �ı�
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
           /* Player playerHealth = collision.gameObject.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }*/
        }
        Destroy(gameObject); // �浹 �� ����ü �ı�
    }
}
