using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Field : MonoBehaviour
{
    public float damagePerSecond = 10f; // �ʴ� ������
    public float damageInterval = 1.0f;
    private bool isPlayerInside = false; // �÷��̾ ���� �ȿ� �ִ��� Ȯ��
    [SerializeField] private GameObject player; // �÷��̾� ��ü ����
    [SerializeField] private float damageTimer = 0f; // Ÿ�̸� ����
    private void Update()
    {
        if (player == null || !isPlayerInside) return;

        // ������ ���ݿ� �����ϸ� �������� ����
        if (damageTimer >= damageInterval)
        {
            Debug.Log("Get Blazed");
            ApplyDamage();
            damageTimer = 0f; // Ÿ�̸� �ʱ�ȭ
        }
        else
        {
            // Ÿ�̸Ӹ� ������Ŵ
            damageTimer += Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Area");
        if (other.CompareTag("Player")) // �÷��̾ ���Դ��� Ȯ��
        {
            isPlayerInside = true;
            player = other.gameObject; // �÷��̾� ��ü ����
            damageTimer = 0.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // �÷��̾ �������� Ȯ��
        {
            isPlayerInside = false;
            player = null;
        }
    }

    private void ApplyDamage()
    {
        if (player != null)
        {
            player.GetComponent<PlayerStatus>().DecreaseHealth(damagePerSecond);
        }
    }



}


