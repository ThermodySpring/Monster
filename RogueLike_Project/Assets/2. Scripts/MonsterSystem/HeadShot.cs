using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class HeadShot : MonoBehaviour
{

    [SerializeField] MonsterBase monsterBase;
    [SerializeField] private AudioClip hitSound; // �ǰ� ����

    [SerializeField] float criticalDamage = 2f;
    private PlayerStatus playerStatus;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        InitializeComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeComponents()
    {
        

        // �÷��̾� ���� ��������
        playerStatus = ServiceLocator.Current.Get<IGameModeService>()
            .GetPlayerCharacter().GetComponent<PlayerStatus>();

        // ����� �ҽ� ��������
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Debug.Log("Hit by " + other.gameObject.name);
            float bulletDamage = other.gameObject.GetComponent<Projectile>().bulletDamage;
            float totalDamage = bulletDamage * playerStatus.GetAttackDamage() / 100 * criticalDamage;
            ApplyDamage(totalDamage);
        }
    }
    private void ApplyDamage(float damage)
    {
        // �� ü�� ����
        monsterBase.TakeDamage(damage);

        // �̺�Ʈ Ʈ����
        EventManager.Instance.TriggerMonsterCriticalDamagedEvent();

    }
}
