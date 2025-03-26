using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeapon : MonoBehaviour
{
    [Header("���� �⺻ ����")]
    [SerializeField] private string weaponName;
    [SerializeField] private Collider weaponCollider;
    [SerializeField] private bool isCollisionEnabled = false;
    [SerializeField] private LayerMask targetLayers; // �÷��̾� ���̾�
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioClip hitSound;

    [Header("������ ����")]
    [SerializeField] private float baseDamage = 10f;
    private float currentDamage;

    private Ransomware owner; // ������ ������ (����)
    private AudioSource audioSource;
    private List<GameObject> hitTargets = new List<GameObject>();

    private void Awake()
    {
        // �ݶ��̴� Ȯ�� �Ǵ� �߰�
        if (weaponCollider == null)
            weaponCollider = GetComponent<Collider>();

        if (weaponCollider != null)
            weaponCollider.isTrigger = true;

        // ����� �ҽ� Ȯ�� �Ǵ� �߰�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // �ʱ� ������ ����
        currentDamage = baseDamage;
    }

    private void Start()
    {
        // ������(����) ã��
        owner = GetComponentInParent<Ransomware>();

        // ����Ʈ ���´� ��Ȱ��ȭ
        DisableCollision();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollisionEnabled)
            return;

        // ���̾� üũ
        if (((1 << other.gameObject.layer) & targetLayers) == 0)
            return;

        // �̹� Ÿ���� ������� Ȯ��
        if (hitTargets.Contains(other.gameObject))
            return;

        // �÷��̾�� ������ ����
        PlayerStatus playerHealth = other.GetComponent<PlayerStatus>();
        if (playerHealth != null)
        {
            playerHealth.DecreaseHealth(GetDamage());
            hitTargets.Add(other.gameObject);

            // Ÿ�� ȿ��
            PlayHitEffects(other.ClosestPoint(transform.position));
        }
    }

    // ������ ��� (������ ����, Ư�� ȿ�� �� ���)
    public float GetDamage()
    {
        float finalDamage = currentDamage;

        // ������ �ְ�, ������ ���¿� ���� ������ ����
        if (owner != null)
        {
            // ������ ü���� 50% ������ �� ������ ���� (2������)
            if (owner.MonsterStatus.GetHealth() <= owner.MonsterStatus.GetMaxHealth() * 0.5f)
            {
                finalDamage *= 1.5f; // 2������� 50% ������ ����
            }

            // ������ Ư�� ��ų ��� ���� �� ������ ����
            if (owner.Animator.GetCurrentAnimatorStateInfo(0).IsName("SpecialAttack"))
            {
                finalDamage *= 1.3f; // Ư�� ���� �� 30% ������ ����
            }
        }

        return finalDamage;
    }

    // ������ ���� (�ܺο��� ȣ��)
    public void SetDamage(float newDamage)
    {
        currentDamage = newDamage;
    }

    // ������ ���� (����)
    public void ModifyDamage(float multiplier)
    {
        currentDamage = baseDamage * multiplier;
    }

    // ���� �浹 Ȱ��ȭ (�ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void EnableCollision()
    {
        isCollisionEnabled = true;
        hitTargets.Clear();
    }

    // ���� �浹 ��Ȱ��ȭ (�ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void DisableCollision()
    {
        isCollisionEnabled = false;
    }

    // Ÿ�� ȿ�� ���
    private void PlayHitEffects(Vector3 hitPoint)
    {
        // ��ƼŬ ȿ��
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // ���� ȿ��
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
}
