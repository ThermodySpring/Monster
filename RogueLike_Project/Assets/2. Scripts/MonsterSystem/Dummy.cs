using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Dummy : MonoBehaviour
{
    [SerializeField] GameObject UIDamaged;
    public virtual void TakeDamage(float damage)
    {
        // ü�� ���� ó��
        UIDamage uIDamage = Instantiate(UIDamaged, transform.position, Quaternion.identity).GetComponent<UIDamage>();
        uIDamage.damage = damage;
    }
}
