using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MProjectile : MonoBehaviour
{
    
    private float damage; // ����ü�� ���ط�
   
    [Header("Settings")]
    [SerializeField] float lifetime = 20f; // ����ü�� ����
    [SerializeField] float speed = 0.05f; // ����ü�� �ӵ�
    [SerializeField] Vector3 dir = Vector3.zero;

    void Start()
    {
        Destroy(gameObject, lifetime); // ���� �ð� �� ����ü �ı�
    }

    void Update()
    {
        Move();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player takes damage " + damage);
            PlayerStatus playerHealth = other.GetComponent<PlayerStatus>();
            if (playerHealth != null)
            {
                playerHealth.DecreaseHealth(damage);
            }

            
        }
        Destroy(gameObject);
    }

    private void Move()
    {
        transform.Translate(dir * speed * Time.deltaTime);
    }

    public void SetBulletDamage(float attackDamage)
    {
        damage = attackDamage;
        Debug.Log("Bullet damage : "+ damage);
    }

    public void SetDirection(Vector3 dir)
    {
        this.dir = dir; 
    }
}
