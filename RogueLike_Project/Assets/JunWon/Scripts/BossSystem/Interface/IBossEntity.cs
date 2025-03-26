using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��� ���� Ÿ���� �⺻ �������̽�
/// </summary>
public interface IBossEntity
{
    float GetCurrentHealth();
    float GetMaxHealth();
    float GetBaseDamage();
    float GetDamageMultiplier();
    int GetCurrentPhase();
    bool IsInSpecialState();
    Transform GetTransform();
    Animator GetAnimator();
}