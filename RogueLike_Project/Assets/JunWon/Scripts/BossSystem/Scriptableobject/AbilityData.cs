using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityData", menuName = "Abilities/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string abilityName; // ��ų �̸�
    public string abilityDescription; // ��ų ����
    public float cooldown; // ��ٿ� �ð� (��)

    public enum AbilityType { Active, Passive } // ��ų ���� (Active: ��� ����, Passive: ���� ȿ��)
    public AbilityType abilityType;

    public Sprite abilityIcon; // ��ų ������

    // ��Ƽ�� ��ų
    public enum TargetType { Single, Multi, Area } // Ÿ�� ���� (Single: ����, Multi: �ټ�, Area: ����)
    public TargetType targetType;
    public float damage; // ���ݷ�
    public float range; // �����Ÿ�

    // �нú� ��ų
    public enum BuffType { None, Attack, Defense, Speed } // ���� ����
    public BuffType buffType;
    public float buffAmount; // ������

    public AnimationClip abilityAnimation; // ��ų �ִϸ��̼� Ŭ��

    public GameObject effectPrefab; // ��ų ����Ʈ ������

}