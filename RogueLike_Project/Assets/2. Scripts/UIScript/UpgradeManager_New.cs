using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random = UnityEngine.Random;
using InfimaGames.LowPolyShooterPack;
using static UpgradeManager;

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
    CharacterBehaviour player;
    PlayerStatus playerStatus;

    Dictionary<UpgradeTier, Action> upgradeActions;
    

    private void Start()
    {
        StartCoroutine(UpgradeDisplay());

        inputField.onEndEdit.AddListener(OnInputEnd);

        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        playerStatus = player.gameObject.GetComponent<PlayerStatus>();

        upgradeActions = new Dictionary<UpgradeTier, Action>
        {
            {UpgradeTier.common,ApplyCommonUpgrade },
            {UpgradeTier.weapon,ApplyWeaponUpgrade },
        };
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

        player.SetCursorState(false);

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
    WeaponUpgrade weaponTypeInput;
    //SpecialUpgrade specialUpgrade;
    int upgradeResult = -1;

    void OnInputEnd(string input)
    {
        
        if(upgradeTier == UpgradeTier.common && Enum.TryParse(input, true, out commonTypeInput))
        {
            upgradeResult = upgradeType[(int)commonTypeInput];
            inputField.onEndEdit.RemoveListener(OnInputEnd);
        }
        else if(upgradeTier == UpgradeTier.weapon && Enum.TryParse(input,true,out weaponTypeInput))
        {
            upgradeResult = upgradeType[(int)weaponTypeInput];
            inputField.onEndEdit.RemoveListener(OnInputEnd);
        }
        //else if (upgradeTier == UpgradeTier.special && Enum.TryParse(input, out int result))
        //{
        //    upgradeResult = upgradeType[(int)result];
        //}
        else
        {
            Debug.Log("Wrong Input");
            return;
        }
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
                        playerStatus?.IncreaseAttackDamage(10);
                        break;
                    case ATKUGType.AttackSpeed:
                        //���� ����
                        Debug.Log("AttackSpeed");
                        playerStatus?.IncreaseAttackSpeed(1);
                        break;
                    case ATKUGType.ReloadSpeed:
                        //���� ����
                        Debug.Log("ReloadSpeed");
                        playerStatus?.IncreaseReloadSpeed(1);
                        break;
                }
                break;
            case CommonUpgrade.UTIL:
                switch ((UTILUGType)upgradeResult)
                {
                    case UTILUGType.Heath:
                        //���� ����
                        Debug.Log("Heath");
                        playerStatus.IncreaseHealth(1);
                        break;
                    case UTILUGType.MoveSpeed:
                        //���� ����
                        Debug.Log("MoveSpeed");
                        playerStatus.IncreaseMovementSpeed(1);
                        break;
                    
                }
                break;
            case CommonUpgrade.COIN:
                switch ((COINUGType)upgradeResult)
                {
                    case COINUGType.CoinAcquisitonRate:
                        //���� ����
                        Debug.Log("CoinAcquisitonRate");
                        playerStatus.IncreaseCoin(1);
                        break;
                    case COINUGType.PermanentCoinAcquisitionRate:
                        //���� ����
                        Debug.Log("PermanentCoinAcquisitionRate");
                        playerStatus.IncreasePermanentAcquisitionRate(1);
                        break;
                }
                break;
        }
    }
    void ApplyWeaponUpgrade()
    {
        switch(weaponTypeInput)
        {
            case WeaponUpgrade.Blaze:
                switch ((WeaponUpgradeSet)upgradeResult)
                {
                    case WeaponUpgradeSet.damage:
                        WeaponConditionUpgrade(1, 0, 0, 0, 0);
                        break;
                    case WeaponUpgradeSet.interval:
                        WeaponConditionUpgrade(0, 0, 0, 1, 0);
                        break;
                    case WeaponUpgradeSet.effect:
                        WeaponConditionUpgrade(0, 0, 0, 0, 1);
                        break;
                    case WeaponUpgradeSet.probability:
                        WeaponConditionUpgrade(0, 0, 1, 0, 0);
                        break;
                    case WeaponUpgradeSet.duration:
                        WeaponConditionUpgrade(0, 1, 0, 0, 0);
                        break;
                }
                break;
            case WeaponUpgrade.Freeze:
                switch ((WeaponUpgradeSet)upgradeResult)
                {
                    case WeaponUpgradeSet.damage:

                        break;
                    case WeaponUpgradeSet.interval:

                        break;
                    case WeaponUpgradeSet.effect:

                        break;
                    case WeaponUpgradeSet.probability:

                        break;
                    case WeaponUpgradeSet.duration:

                        break;
                }
                break;
            case WeaponUpgrade.Shock:
                switch ((WeaponUpgradeSet)upgradeResult)
                {
                    case WeaponUpgradeSet.damage:

                        break;
                    case WeaponUpgradeSet.interval:

                        break;
                    case WeaponUpgradeSet.effect:

                        break;
                    case WeaponUpgradeSet.probability:

                        break;
                    case WeaponUpgradeSet.duration:

                        break;
                }
                break;
        }
    }
    private void WeaponConditionUpgrade(float dmg, float dur, float prob, float itv, float eff)
    {
        WeaponBehaviour weapon = player.GetInventory().GetEquipped();
        switch (weaponTypeInput)
        {
            case WeaponUpgrade.Blaze:
                Blaze weaponBlaze = weapon.GetComponent<Blaze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponBlaze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponBlaze == null) weapon.AddComponent<Blaze>().StateInitializer(1, 1, 1, 1, 1);
                else weaponBlaze.Upgrade(dmg,dur,prob,itv,eff);
                break;
            case WeaponUpgrade.Freeze:
                Freeze weaponFreeze = weapon.GetComponent<Freeze>();
                if (weapon.GetComponent<WeaponCondition>() != weaponFreeze) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponFreeze == null) weapon.AddComponent<Freeze>().StateInitializer(1, 1, 1, 1, 1);
                else weaponFreeze.Upgrade(dmg, dur, prob, itv, eff);
                break;
            case WeaponUpgrade.Shock:
                Shock weaponShock = weapon.GetComponent<Shock>();
                if (weapon.GetComponent<WeaponCondition>() != weaponShock) Destroy(weapon.GetComponent<WeaponCondition>());
                if (weaponShock == null) weapon.AddComponent<Shock>().StateInitializer(1, 1, 1, 1, 1);
                else weaponShock.Upgrade(dmg, dur, prob, itv, eff);
                break;
            default:
                break;
        }
    }
    //void ApplyWeaponUpgrade2()
    //{
    //    switch()
    //    {
    //        case RareUpgradeSet.damage:
    //        case RareUpgradeSet.probability:
    //        case RareUpgradeSet.duration:
    //        case RareUpgradeSet.interval:
    //        case RareUpgradeSet.effect:
    //    }
    //}

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
        upgradeActions[upgradeTier].Invoke();
        upgradeSuccess.SetActive(true);
        yield return new WaitForSeconds(3f);
        upgradeRootUI.SetActive(false);

        player.SetCursorState(true);

    }

}
