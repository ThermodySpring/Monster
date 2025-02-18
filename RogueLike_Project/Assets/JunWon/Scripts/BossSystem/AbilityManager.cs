using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    // �ν����Ϳ��� ���� AbilityData�� �Ҵ��� �� �ֵ��� ����Ʈ�� ����
    [SerializeField] private List<AbilityData> abilitiesData;

    // �� ��ų�� �̸����� �����Ͽ� ����
    private Dictionary<string, AbilityInstance> abilities;

    private void Awake()
    {
        abilities = new Dictionary<string, AbilityInstance>();
        foreach (var data in abilitiesData)
        {
            if (data != null && !abilities.ContainsKey(data.abilityName))
            {
                abilities.Add(data.abilityName, new AbilityInstance(data));
            }
        }
    }

    /// <summary>
    /// ��ų ��� �õ� (��ٿ� üũ ����)
    /// </summary>
    public bool UseAbility(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            AbilityInstance ability = abilities[abilityName];
            if (ability.IsReady())
            {
                ability.Use();
                return true;
            }
            else
            {
                Debug.Log(abilityName + " ��ų�� ���� ��ٿ� ���Դϴ�. ���� �ð�: " + ability.GetRemainingCooldown());
            }
        }
        else
        {
            Debug.LogWarning("AbilityManager�� " + abilityName + " ��ų�� ��ϵǾ� ���� �ʽ��ϴ�.");
        }
        return false;
    }

    /// Ư�� ��ų�� ���� ��ٿ� �ð� ��ȯ
    public float GetAbilityRemainingCooldown(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetRemainingCooldown();
        }
        return 0;
    }

    /// ������ ���� ������ Ȱ�� ��ų�� ������Ʈ�ϰ� �ʹٸ� �� �޼��带 Ȱ���� �� �ֽ��ϴ�.
    /// ���� ���, ����� ���� Ư�� ��ų�� ��Ȱ��ȭ/Ȱ��ȭ�� �� �ֽ��ϴ�.
    public void SetAbilityActive(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            // active �÷��׸� AbilityData�� AbilityInstance ���� �߰��Ͽ� ó���� �� �ֽ��ϴ�.
            // ���÷� AbilityData�� bool isActive�� �߰��� ��, �̸� Ȱ���� �� �ֽ��ϴ�.
            abilities[abilityName].Activate();
        }
    }
    public void SetAbilityInactive(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            abilities[abilityName].Deactivate();
        }
    }
    public float GetAbiltiyDmg(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetDmg();
        }
        return 0;
    }

    /// <summary>
    /// Ư�� ��ų(�����Ƽ)�� �������� ��ȯ
    /// </summary>
    public GameObject GetAbilityPrefab(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            return abilities[abilityName].GetPrefab();
        }
        return null;
    }

    public void SetMaxCoolTime(string abilityName)
    {
        if (abilities.ContainsKey(abilityName))
        {
            abilities[abilityName].InitializeWithMaxCooldown();
        }

    }

}
