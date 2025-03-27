using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager_New : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject[] commonUpgradeSet;
    [SerializeField] GameObject[] weaponUpgradeSet;
    [SerializeField] GameObject[] specialUpgradeSet;

    public UpgradeTier upgradeTier;
    
    private void Start()
    {
        StartCoroutine(UpgradeDisplay());
    }
    IEnumerator UpgradeDisplay()
    {
        yield return new WaitForSeconds(0.2f);
        //1. ���׷��̵� Ƽ� ����ߵ� ����Ʈ��
        //2. Ÿ�� ����Ʈ���� ������ ���׷��̵� ��� ����
        //3. ��ǲ�ʵ� Ȱ��ȭ
        //4. ����Ű �Է½� ���� ����
    }
}
