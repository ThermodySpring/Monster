using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPart : MonoBehaviour
{
    [SerializeField] private string partName; // ���� �̸�
    [SerializeField] private Collider collider; // ������ Collider
    [SerializeField] private float damageMultiplier = 1.0f; // ������ ����

    public string PartName => partName;
    public Collider Collider => collider;
    public float DamageMultiplier => damageMultiplier;
}
