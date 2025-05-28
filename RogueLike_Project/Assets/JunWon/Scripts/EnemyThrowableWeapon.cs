using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrowableWeapon : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private float explosionForce = 15f;
    [SerializeField] private LayerMask targetLayers = -1; // ��� ���̾� �⺻��

    [Header("Visual & Audio")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float cameraShakeIntensity = 0.5f;
    [SerializeField] private float cameraShakeDuration = 0.3f;

    [Header("Advanced Settings")]
    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private bool applyKnockback = true;
    [SerializeField] private AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0.3f);
    [SerializeField] private float minDamagePercent = 0.2f;

    private bool hasExploded = false;
    private Rigidbody rb;
    private AudioSource audioSource;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        // ����� �ҽ��� ������ ����
        if (audioSource == null && explosionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �ٴ��̳� ���� �浹 �� ����
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Boom!");
            Explode(collision.contacts[0].point);
        }
    }

    // �ܺο��� ������ Ʈ������ �� �ִ� �޼���
    public void TriggerExplosion()
    {
        Explode(transform.position);
    }

    private void Explode(Vector3 explosionPoint)
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"Explosion at {explosionPoint} with radius {explosionRadius}");

        // 1. ���� ���� �� ��� �ݶ��̴� Ž��
        Collider[] hitColliders = Physics.OverlapSphere(explosionPoint, explosionRadius, targetLayers);

        List<GameObject> affectedObjects = new List<GameObject>();

        foreach (Collider hitCollider in hitColliders)
        {
            // �ڱ� �ڽ��� ����
            if (hitCollider.gameObject == gameObject) continue;

            // �ߺ� ó�� ���� (���� ������Ʈ�� ���� �ݶ��̴�)
            if (affectedObjects.Contains(hitCollider.gameObject)) continue;
            affectedObjects.Add(hitCollider.gameObject);

            // ���� �������κ����� �Ÿ� ���
            float distance = Vector3.Distance(explosionPoint, hitCollider.transform.position);
            float normalizedDistance = Mathf.Clamp01(distance / explosionRadius);

            // �Ÿ��� ���� ������ ���� ����
            float damageMultiplier = damageFalloff.Evaluate(normalizedDistance);
            damageMultiplier = Mathf.Max(damageMultiplier, minDamagePercent);

            float finalDamage = explosionDamage * damageMultiplier;

            // 2. �÷��̾� ������ ó��
            PlayerStatus playerStatus = hitCollider.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.DecreaseHealth(finalDamage);
                Debug.Log($"Player takes {finalDamage} explosion damage");
            }

            // 3. �ٸ� ���͵鿡�Ե� ������ (��ų ����)
            //MonsterBase monster = hitCollider.GetComponent<MonsterBase>();
            //if (monster != null)
            //{
            //    monster.TakeDamage(finalDamage * 0.5f, true); // ���ʹ� ���� ������
            //    Debug.Log($"Monster {monster.name} takes {finalDamage * 0.5f} explosion damage");
            //}

            // 4. �˹� ȿ�� ����
            if (applyKnockback)
            {
                ApplyKnockback(hitCollider, explosionPoint, normalizedDistance);
            }

            // 5. �ı� ������ ������Ʈ ó��
            //DestructibleObject destructible = hitCollider.GetComponent<DestructibleObject>();
            //if (destructible != null)
            //{
            //    destructible.TakeDamage(finalDamage);
            //}
        }

        // 6. �ð��� ȿ��
        CreateExplosionEffect(explosionPoint);

        // 7. ����� ȿ��
        PlayExplosionSound();

        // 8. ī�޶� ����ũ (�ִٸ�)
        TriggerCameraShake();

        // 9. ��ź ������Ʈ ����
        if (destroyOnImpact)
        {
            StartCoroutine(DestroyAfterEffect());
        }

    }

    private void ApplyKnockback(Collider target, Vector3 explosionPoint, float normalizedDistance)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // ���� �������� Ÿ�������� ���� ���
            Vector3 knockbackDirection = (target.transform.position - explosionPoint).normalized;

            // Y�� ������ �ణ �߰��ؼ� ���� Ƣ������� ȿ��
            knockbackDirection.y = Mathf.Max(knockbackDirection.y, 0.3f);

            // �Ÿ��� ���� �� ����
            float knockbackForce = explosionForce * (1f - normalizedDistance);

            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        // �÷��̾� Ư�� ó�� (��� ȿ�� ��)
        PlayerControl playerControl = target.GetComponent<PlayerControl>();
        if (playerControl != null)
        {
            Vector3 airborneDirection = (target.transform.position - explosionPoint).normalized;
            StartCoroutine(playerControl.AirBorne(airborneDirection));
        }
    }

    private void CreateExplosionEffect(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, position, Quaternion.identity);

            // ����Ʈ ������ ���� (���� �ݰ濡 ����)
            float scale = explosionRadius / 5f; // �⺻ �ݰ� 5�� �������� ������ ����
            effect.transform.localScale = Vector3.one * scale;

            // ����Ʈ �ڵ� ����
            Destroy(effect, 3f);
        }
    }

    private void PlayExplosionSound()
    {
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }

    private void TriggerCameraShake()
    {
        // ī�޶� ����ũ �ý����� �ִٸ� ȣ��
        // CameraShakeManager.Instance?.Shake(cameraShakeIntensity, cameraShakeDuration);

        // �Ǵ� �̺�Ʈ�� ó��
        // EventManager.Instance?.TriggerCameraShake(cameraShakeIntensity, cameraShakeDuration);
    }

    private IEnumerator DestroyAfterEffect()
    {
        // ���� ����Ʈ�� ���尡 ����� �ð��� ��ٸ�
        yield return new WaitForSeconds(0.1f);

        // ������ ��Ȱ��ȭ (�ð������� �����)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // �ݶ��̴� ��Ȱ��ȭ (�߰� �浹 ����)
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // ���� ��� �Ϸ� �� ���� ����
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    // ����׿� ����� �׸��� (�����Ϳ�����)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 0.5f);
    }

    // ��Ÿ�ӿ��� ���� ���� ����
    public void SetExplosionRadius(float radius)
    {
        explosionRadius = radius;
    }

    public void SetExplosionDamage(float damage)
    {
        explosionDamage = damage;
    }

    public void SetExplosionForce(float force)
    {
        explosionForce = force;
    }
}
