using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffField : MonoBehaviour
{
    public float duration = 5f;
    
    [SerializeField]
    private float healAmount = 5f; // �ʴ� ȸ����
    public float healInterval = 1f; // HP ȸ�� ���� (��)

    [SerializeField]
    private float healTimer = 0f;


    [SerializeField]
    private LayerMask targetLayer; // ���� Layer

    void Start()
    {
        Destroy(gameObject, duration);
    }

    void OnTriggerStay(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            MonsterStatus monster = other.GetComponent<MonsterStatus>();
            
            if (monster != null)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healInterval)
                {
                    monster.IncreaseHealth(healAmount);
                    Debug.Log("Heal");
                    healTimer = 0f;
                }
            }
        }
    }

}
