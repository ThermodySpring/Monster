using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �⺻ ���� �������̽�
/// </summary>
public interface IWeapon
{
    void EnableCollision();
    void DisableCollision();
    void SetDamage(float damage);
    void UpdateDamageFromSource();
    void ApplyHitEffect(Vector3 hitPoint, GameObject target);
    GameObject GetGameObject();
}
