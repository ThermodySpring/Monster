using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance : MonoBehaviour
{
    public AbilityData data;
    private float lastUsedTime;

    bool isActive = false;

    public AbilityInstance(AbilityData data)
    {
        isActive = false;
        this.data = data;
        lastUsedTime = -data.cooldown;
    }

    #region Skill Activate Methon
    public void Activate()
    {
        isActive = true;
        Debug.Log(data.abilityName + " ��ų Ȱ��ȭ��");
    }

    public void Deactivate()
    {
        isActive = false;
        Debug.Log(data.abilityName + " ��ų ��Ȱ��ȭ��");
    }
    #endregion

    // ��ų ��� ���� ���� üũ (Time.unscaledTime ���)
    public bool IsReady()
    {
        return Time.unscaledTime >= lastUsedTime + data.cooldown;
    }

    // ��ų ��� ó�� (��ٿ� ����)
    public void Use()
    {
        if (!isActive)
        {
            Debug.Log(data.abilityName + " ��ų�� ���� ��Ȱ�� �����Դϴ�.");
            return;
        }

        if (IsReady())
        {
            lastUsedTime = Time.unscaledTime;
            Debug.Log(data.abilityName + " ��ų ���!");
            // �߰� ��ų ���� ����
        }
    }

    // ���� ��ٿ� �ð� ��ȯ
    public float GetRemainingCooldown()
    {
        float remain = (lastUsedTime + data.cooldown) - Time.unscaledTime;
        return remain > 0 ? remain : 0;
    }

    public void InitializeWithMaxCooldown()
    {
        lastUsedTime = Time.unscaledTime;
    }

    public float GetDmg()
    {
        return data.damage;
    }
}
