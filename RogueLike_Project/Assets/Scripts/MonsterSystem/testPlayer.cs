using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testPlayer : MonoBehaviour
{
    private RangedMonster rangedMonster; // RangedMonster�� ������ �� �ִ� ����
    private MeeleMonster meeleMonster; // RangedMonster�� ������ �� �ִ� ����
    [SerializeField] bool attack;

    void Start()
    {
        attack = false;
        // RangedMonster ��ü�� ã���ϴ�.
        rangedMonster = FindObjectOfType<RangedMonster>();
        meeleMonster = FindObjectOfType<MeeleMonster>();

        if (rangedMonster == null)
        {
            Debug.LogError("No RangedMonster found in the scene.");
        }
        if (meeleMonster == null)
        {
            Debug.LogError("No MeeleMonster found in the scene.");

        }
    }

    void Update()
    {
        // ���� ���, Ű���� �Է¿� ���� �������� �� �� �ֽ��ϴ�.
        if (Input.GetKeyDown(KeyCode.N))
        {
            // RangedMonster�� �Ҵ�Ǿ� ������ TakeDamage �޼��带 ȣ���մϴ�.
            rangedMonster.TakeDamage(5, gameObject.transform); // ��: 5�� �������� �ݴϴ�.
            meeleMonster.TakeDamage(5, gameObject.transform);
            Debug.Log("Damage dealt to RangedMonster.");
            Debug.Log("Damage dealt to meeleMonster.");
        }
    }
}
