using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class UpgradeManager_New : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;

    public UpgradeTier upgradeTier;
    GameObject[] UpgradeSet;
    public List<int> upgradeType;

    bool isWaitingInput = false;
    
    private void Start()
    {
        StartCoroutine(UpgradeDisplay());

        upgradeTier = UpgradeTier.common;

        inputField.onEndEdit.AddListener(OnInputEnd);
    }
    IEnumerator UpgradeDisplay()
    {
        upgradeType = new List<int>();
        yield return new WaitForSeconds(0.2f);

        // 1. ���׷��̵� Ƽ��� ����ߵ� ����Ʈ��
        switch (upgradeTier)
        {
            case UpgradeTier.common: UpgradeSet = commonUpgradeSet; break;
            case UpgradeTier.weapon: UpgradeSet = weaponUpgradeSet; break;
            case UpgradeTier.special: UpgradeSet = specialUpgradeSet; break;
        }

        if (UpgradeSet == null || UpgradeSet.Length == 0)
        {
            Debug.LogError("UpgradeSet�� ��� �ֽ��ϴ�. Inspector���� Ȯ���ϼ���!");
            yield break;
        }

        // 2. Ÿ�� ����Ʈ���� ������ ���׷��̵� ��� ����
        foreach (GameObject upgrade in UpgradeSet)
        {
            List<Transform> directChildren = new List<Transform>();

            foreach (Transform child in upgrade.transform)
            {
                directChildren.Add(child);
            }

            if (directChildren.Count > 0)
            {
                int randIdx = Random.Range(0, directChildren.Count);
                directChildren[randIdx].gameObject.SetActive(true);
                upgradeType.Add(randIdx);
            }
        }

        // 3. ��ǲ�ʵ� Ȱ��ȭ
        inputField.transform.parent.gameObject.SetActive(true);
        isWaitingInput = true;
    }
    
    void OnInputEnd(string input)
    {
        if(upgradeTier == UpgradeTier.common && Enum.TryParse(input, out CommonUpgrade result))
        {
            
        }
        
    }
    IEnumerator EndUpgrade()
    {
        yield return new WaitForEndOfFrame();
    }

}
