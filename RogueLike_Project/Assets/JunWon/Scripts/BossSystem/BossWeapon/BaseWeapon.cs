using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseWeapon : MonoBehaviour, IWeapon
{
    [Header("���� �⺻ ����")]
    [SerializeField] protected string weaponName;
    [SerializeField] protected Collider weaponCollider;
    [SerializeField] protected bool isCollisionEnabled = false;
    [SerializeField] protected LayerMask targetLayers; // �÷��̾� ���̾�
    [SerializeField] protected ParticleSystem hitEffect;
    [SerializeField] protected AudioClip hitSound;

    [Header("������ ����")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected float damageMultiplier = 1f;

    [Header("ȿ�� ����")]

    protected float currentDamage;
    protected IBossEntity bossOwner;
    protected AudioSource audioSource;
    [SerializeField] protected List<GameObject> hitTargets = new List<GameObject>();

    protected virtual void Awake()
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

    protected virtual void Start()
    {
        // ���� �������� ���� ã��
        FindBossOwner();

        // ������ �ʱ�ȭ
        UpdateDamageFromSource();

        // �⺻ ���´� ��Ȱ��ȭ
        DisableCollision();
    }

    protected virtual void FindBossOwner()
    {
        // �θ� ������Ʈ���� IBossEntity �������̽� ����ü ã��
        Transform current = transform.parent;

        while (current != null)
        {
            IBossEntity boss = current.GetComponent<IBossEntity>();
            if (boss != null)
            {
                bossOwner = boss;
                break;
            }
            current = current.parent;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Meele Attack is activated");
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
            playerHealth.DecreaseHealth(CalculateDamage());
            hitTargets.Add(other.gameObject);

            // Ÿ�� ȿ��
            ApplyHitEffect(other.ClosestPoint(transform.position), other.gameObject);

            // �߰� ȿ�� ����
            ApplyWeaponEffects(other.gameObject, other.ClosestPoint(transform.position));
        }
    }

    public virtual void EnableCollision()
    {
        isCollisionEnabled = true;
        hitTargets.Clear();
    }

    public virtual void DisableCollision()
    {
        isCollisionEnabled = false;
    }

    public virtual void SetDamage(float damage)
    {
        currentDamage = damage;
    }

    public virtual void UpdateDamageFromSource()
    {
        if (bossOwner != null)
        {
            // �����κ��� �⺻ ������ ��������
            baseDamage = bossOwner.GetBaseDamage();

            // ������ ���� ������/���¿� ���� ��Ƽ�ö��̾� ����
            //damageMultiplier = bossOwner.GetDamageMultiplier();

            // ���� ������ ���
            currentDamage = baseDamage * damageMultiplier;
        }
    }

    protected virtual float CalculateDamage()
    {
        float finalDamage = currentDamage;

        // ������ Ư�� ���¸� �߰� ������ ����
        if (bossOwner != null && bossOwner.IsInSpecialState())
        {
            finalDamage *= 1.3f;
        }

        return finalDamage;
    }

    public virtual void ApplyHitEffect(Vector3 hitPoint, GameObject target)
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

    protected virtual void ApplyWeaponEffects(GameObject target, Vector3 hitPoint)
    {
        //foreach (WeaponEffect effect in weaponEffects)
        //{
        //    effect.ApplyEffect(target, hitPoint);
        //}
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    // ������ �����
    protected virtual void OnDrawGizmos()
    {
        if (weaponCollider != null && isCollisionEnabled)
        {
            Gizmos.color = Color.red;

            if (weaponCollider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (weaponCollider is CapsuleCollider capsuleCollider)
            {
                // ĸ�� �ݶ��̴� �ð�ȭ
                // (����ȭ�� ���� ����)
            }
        }
    }



}