using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� ���� Ÿ���� �⺻ �������̽�
/// </summary>
public interface IBossEntity
{
    float GetBaseDamage();
    float GetDamageMultiplier();
    bool IsInSpecialState();
}