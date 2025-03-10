using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalShadow : RangedMonster
{
    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Settings")]
    [SerializeField] private float health = 30f;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private Material glitchMaterial;

    private float spawnTime;
    private bool isDying = false;
    private Ransomware creator; // ������ ���� ����

    // ������ ������ �ʱ�ȭ �޼���
    public void Initialize(Ransomware boss)
    {
        creator = boss;
        spawnTime = Time.time;

        // �۸�ġ ȿ�� ����
        ApplyGlitchEffect();

        // ���� �ð� �� �ڵ� �ı� �ڷ�ƾ ����
        StartCoroutine(LifetimeRoutine());
    }

    protected override void Start()
    {
        base.Start();

        // RangedMonster�� Start() ȣ�� �� �߰� ����
        attackCooldown = 3f; // �� ���� �����ϵ��� ��ٿ� ����
        chaseSpeed = 4.5f; // �ణ �� ������ �̵�
        attackRange = 10f; // ���� ���� ����

        // �̵� �� �ִϸ��̼� ����
        if (nmAgent != null)
        {
            nmAgent.speed = chaseSpeed;
        }
    }

    protected override void Update()
    {
        base.Update();

        // �߰��� ������ �����츸�� �ð� ȿ�� ������Ʈ ����
        UpdateGlitchEffect();
    }

    // ������ ó�� �������̵�
    public virtual void TakeDamage(float damage, bool showDamage = true, bool flagForExecution = false)
    {
        if (isDying) return;

        health -= damage;

        // ������ ǥ�� (RangedMonster���� ���)
        if (showDamage)
        {
            base.TakeDamage(damage, true);
        }

        // �ǰ� ȿ��
        StartCoroutine(DamageFlashRoutine());

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;

        // ��� ȿ�� ����
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // �������� ������ �ı� �˸�
        OnShadowDestroyed?.Invoke(gameObject);

        // �ı� �� �ִϸ��̼�/ȿ�� ǥ��
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // ��� �ִϸ��̼� ����
        SetAnimatorState(State.DIE);

        // NavMeshAgent ����
        if (nmAgent != null)
        {
            nmAgent.isStopped = true;
        }

        // 1�� ��� (��� �ִϸ��̼� ���)
        yield return new WaitForSeconds(1f);

        // ������Ʈ �ı�
        Destroy(gameObject);
    }

    private IEnumerator DamageFlashRoutine()
    {
        // �ǰ� �� ��Ƽ���� ������
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_FlashIntensity", 1f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                material.SetFloat("_FlashIntensity", 0f);
            }
        }
    }

    private void ApplyGlitchEffect()
    {
        // ��� �������� �۸�ġ ���̴� ����
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (glitchMaterial != null)
            {
                // ���� ��Ƽ������ �۸�ġ ��Ƽ����� ��ü
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterials[i] = new Material(glitchMaterial);
                }
                renderer.materials = newMaterials;
            }
            else
            {
                // �۸�ġ ��Ƽ������ ���� ��� ���� ��Ƽ���� �۸�ġ ȿ�� �Ӽ� �߰�
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_GlitchIntensity"))
                    {
                        material.SetFloat("_GlitchIntensity", 0.3f);
                    }
                    if (material.HasProperty("_DistortionAmount"))
                    {
                        material.SetFloat("_DistortionAmount", 0.02f);
                    }
                }
            }
        }
    }

    private void UpdateGlitchEffect()
    {
        // ���� ���� �ð��� ���� �۸�ġ ȿ�� ��ȭ
        float remainingLifetimePercent = 1f - ((Time.time - spawnTime) / lifetime);
        float glitchIntensity = Mathf.Lerp(0.8f, 0.3f, remainingLifetimePercent);

        // ���� ü�¿����� �۸�ġ ȿ�� ��ȭ
        float healthPercent = health / 30f; // �ʱ� ü�� ��� ����
        glitchIntensity = Mathf.Max(glitchIntensity, Mathf.Lerp(0.8f, 0.3f, healthPercent));

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_GlitchIntensity"))
                {
                    material.SetFloat("_GlitchIntensity", glitchIntensity);
                }
            }
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (!isDying)
        {
            Die();
        }
    }

    public override void FireEvent()
    {
        base.FireEvent();

        // �߰� ȿ��: �߻� �� �۸�ġ ȿ�� ��ȭ
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_GlitchIntensity"))
                {
                    // �߻� ���� �۸�ġ ȿ�� ��ȭ
                    StartCoroutine(FireGlitchEffect(material));
                }
            }
        }
    }

    private IEnumerator FireGlitchEffect(Material material)
    {
        float originalIntensity = material.GetFloat("_GlitchIntensity");
        material.SetFloat("_GlitchIntensity", 1.0f);
        yield return new WaitForSeconds(0.2f);
        material.SetFloat("_GlitchIntensity", originalIntensity);
    }
}
