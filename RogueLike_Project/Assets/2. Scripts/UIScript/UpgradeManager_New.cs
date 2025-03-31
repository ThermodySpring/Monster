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
    [SerializeField] GameObject upgradeRootUI;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;
    [SerializeField] GameObject upgradeProcessing;
    [SerializeField] GameObject upgradeSuccess;

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
    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Escape) && isWaitingInput)
    //    {
    //        isWaitingInput = false;
    //        OnInputEnd(inputField.text);
    //    }
    //}
    public IEnumerator UpgradeDisplay()
    {
        yield return new WaitForEndOfFrame();
        upgradeRootUI.SetActive(true);

        inputField.transform.parent.gameObject.SetActive(false);
        upgradeProcessing.SetActive(false);
        upgradeSuccess.SetActive(false);

        if(UpgradeSet != null)
        {
            foreach (GameObject upgrade in UpgradeSet)
            {
                upgrade.SetActive(false);
            }

        }

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
                child.gameObject.SetActive(false);
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

    CommonUpgrade commonTypeInput;
    int upgradeResult = -1;

    void OnInputEnd(string input)
    {
        
        if(upgradeTier == UpgradeTier.common && Enum.TryParse(input, true, out commonTypeInput))
        {
            upgradeResult = upgradeType[(int)commonTypeInput];
            inputField.onEndEdit.RemoveListener(OnInputEnd);
        }
        else
        {
            Debug.Log("�߸��� �Է�");
            return;
        }
        //else if (upgradeTier == UpgradeTier.weapon && Enum.TryParse(input, out int result))
        //{
        //    upgradeResult = upgradeType[(int)result];
        //}
        //else if (upgradeTier == UpgradeTier.special && Enum.TryParse(input, out int result))
        //{
        //    upgradeResult = upgradeType[(int)result];
        //}
        StartCoroutine(EndUpgrade());
    }
    void ApplyCommonUpgrade()
    {
        switch (commonTypeInput)
        {
            case CommonUpgrade.ATK:
                switch ((ATKUGType)upgradeResult)
                {
                    case ATKUGType.Damage:
                        //���� ����
                        Debug.Log("Damage up");
                        break;
                    case ATKUGType.AttackSpeed:
                        //���� ����
                        Debug.Log("AttackSpeed");
                        break;
                    case ATKUGType.ReloadSpeed:
                        //���� ����
                        Debug.Log("ReloadSpeed");
                        break;
                }
                break;
            case CommonUpgrade.UTIL:
                switch ((UTILUGType)upgradeResult)
                {
                    case UTILUGType.Heath:
                        //���� ����
                        Debug.Log("Heath");
                        break;
                    case UTILUGType.MoveSpeed:
                        //���� ����
                        Debug.Log("MoveSpeed");
                        break;
                    
                }
                break;
            case CommonUpgrade.COIN:
                switch ((COINUGType)upgradeResult)
                {
                    case COINUGType.CoinAcquisitonRate:
                        //���� ����
                        Debug.Log("CoinAcquisitonRate");
                        break;
                    case COINUGType.PermanentCoinAcquisitionRate:
                        //���� ����
                        Debug.Log("PermanentCoinAcquisitionRate");
                        break;
                }
                break;
        }
    }

    IEnumerator EndUpgrade()
    {
        yield return new WaitForEndOfFrame();
        upgradeProcessing.SetActive(true);
        TMP_Text[] upgradeProcessingText = upgradeProcessing.GetComponentsInChildren<TMP_Text>();
        int progress = 0;
        string progressBarText = "|";
        while(progress < 100)
        {
            progress += Random.Range(0, 16);
            if(progress > 100) progress = 100;

            int barCount = (int)(progress / 100f * 20);
            progressBarText = "|" + new string('��', barCount) + new string('��', (20-barCount)) + "|";
            upgradeProcessingText[1].text = progressBarText;
            upgradeProcessingText[2].text = $"{progress} / 100%";

            yield return new WaitForSeconds(0.15f);
        }
        yield return new WaitForSeconds(0.2f);
        ApplyCommonUpgrade();
        upgradeSuccess.SetActive(true);
        yield return new WaitForSeconds(3f);
        upgradeRootUI.SetActive(false);

    }

}
