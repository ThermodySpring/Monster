using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public interface IBossForm
{
    // �ĺ��� �� �⺻ ����
    string FormName { get; }
    BossForm FormType { get; }

    // ����������Ŭ �޼���
    void Initialize(UnknownVirusBoss controller);
    void Activate();
    void Deactivate();

    // ���� ����
    void SaveState();
    void LoadState();

    // ���� ����
    void HandleAttack();
    void HandleSpecialAbility(string abilityName);
    void HandleMovement(Vector3 targetPosition);

    // ������ �� ü�� ����
    void TakeDamage(float damage, bool showEffect);
    float GetCurrentHealthRatio();

    // AI ����
    float EvaluateFormEffectiveness(BossCombatContext context);
    BossForm SuggestNextForm(BossCombatContext context);
}
