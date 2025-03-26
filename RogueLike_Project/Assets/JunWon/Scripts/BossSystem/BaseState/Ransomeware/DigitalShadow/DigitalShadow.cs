using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DigitalShadow : RangedMonster
{
    public delegate void ShadowDestroyedHandler(GameObject shadow);
    public event ShadowDestroyedHandler OnShadowDestroyed;

    [Header("Digital Shadow Settings")]
    [SerializeField] private float health = 30f;
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private Material glitchMaterial;

    [Header("AI Movement Settings")]
    [SerializeField] private float minDistanceFromPlayer = 5f; // �÷��̾�κ��� �ּ� �Ÿ�
    [SerializeField] private float maxDistanceFromPlayer = 10f; // �÷��̾�κ��� �ִ� �Ÿ�
    [SerializeField] private float minDistanceFromOthers = 3f; // �ٸ� �׸��ڷκ��� �ּ� �Ÿ�
    [SerializeField] private float positionUpdateInterval = 2f; // ��ġ ������ ����
    [SerializeField] private float flankingOffset = 45f; // ���� ������ ���� ���� ������

    [Header("Random Speed Settings")]
    [SerializeField] private float minSpeed = 3.5f; // �ּ� �̵� �ӵ�
    [SerializeField] private float maxSpeed = 5.5f; // �ִ� �̵� �ӵ�
    [SerializeField] private float speedChangeInterval = 3f; // �ӵ� ���� ���� (��)
    [SerializeField] private float speedChangeVariation = 1f; // ������ ���� ��ȭ��

    private float spawnTime;
    private bool isDying = false;
    private Ransomware creator; // ������ ���� ����
    private bool isLastStandFragment = false; // �߾� ���Ͽ� �п� �������� ����
    private float lastPositionUpdateTime = 0f;
    private int shadowID; // �׸��� ���� ID (���� ����)
    private static int nextShadowID = 0; // ���� �׸��ڿ� �Ҵ��� ID

    // ������ ������ �ʱ�ȭ �޼���
    public void Initialize(Ransomware boss)
    {
        creator = boss;
        spawnTime = Time.time;
        shadowID = nextShadowID++;

        // �۸�ġ ȿ�� ����
        ApplyGlitchEffect();

        // �ڿ������� �������� ���� �ʱ� ����
        InitializeMovementBehavior();

        // ���� �ð� �� �ڵ� �ı� �ڷ�ƾ ����
        StartCoroutine(LifetimeRoutine());

        // �ӵ� ����ȭ �ڷ�ƾ ����
        StartCoroutine(RandomizeSpeedRoutine());
    }

    // �߾� ���Ͽ� �п� ���� ����
    public void SetAsLastStandFragment(bool value)
    {
        isLastStandFragment = value;

        // �߾� ���Ͽ��̸� ü�°� ���ݷ� ����
        if (isLastStandFragment)
        {
            monsterStatus.SetHealth(1.5f * monsterStatus.GetMaxHealth()); 
            monsterStatus.SetAttackDamage(1.2f * monsterStatus.GetAttackDamage());
            attackCooldown *= 0.8f; // ���� �ֱ� 0.8�� (�� ���� ����)

            // �ð��� ȿ�� ��ȭ
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_GlitchIntensity"))
                    {
                        material.SetFloat("_GlitchIntensity", 0.5f); // �⺻ ȿ������ �� ���� �۸�ġ
                    }

                    // �� ���� ���� ����
                    if (material.HasProperty("_Color"))
                    {
                        Color originalColor = material.GetColor("_Color");
                        material.SetColor("_Color", new Color(
                            Mathf.Min(originalColor.r + 0.3f, 1.0f),
                            Mathf.Max(originalColor.g - 0.2f, 0.0f),
                            Mathf.Max(originalColor.b - 0.2f, 0.0f),
                            originalColor.a
                        ));
                    }
                }
            }

            // ��ƼŬ �ý��� ���� (�ִ� ���)
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startColor = new Color(1.0f, 0.3f, 0.3f); // �� ���� ��ƼŬ
                main.startLifetime = main.startLifetime.constant * 1.5f; // ��ƼŬ ���ӽð� ����
                main.startSize = main.startSize.constant * 1.2f; // ��ƼŬ ũ�� ����
            }

            // �߾� ���Ͽ����� �ӵ� ������ �� �а� ����
            minSpeed *= 0.9f;
            maxSpeed *= 1.2f;
        }
    }

    // �ӵ��� �ֱ������� �����ϰ� �����ϴ� �ڷ�ƾ
    private IEnumerator RandomizeSpeedRoutine()
    {
        // �ʱ� ������ (��� �׸��ڰ� ���ÿ� �ӵ��� �������� �ʵ���)
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        while (!isDying)
        {
            if (nmAgent != null && nmAgent.enabled)
            {
                // ���ο� ���� �ӵ� ����
                float newSpeed = Random.Range(minSpeed, maxSpeed);
                nmAgent.speed = newSpeed;
                chaseSpeed = newSpeed; // RangedMonster���� �����ϴ� ������ ������Ʈ

                // ID�� ���� �ణ �ٸ� �ӵ� ���� ���
                if (shadowID % 3 == 0)
                {
                    // �ӵ� ��ȭ�� �� �ް��� ����
                    nmAgent.acceleration = Random.Range(8f, 12f);
                }
                else if (shadowID % 3 == 1)
                {
                    // �ӵ� ��ȭ�� �ε巯�� ����
                    nmAgent.acceleration = Random.Range(6f, 9f);
                }
                else
                {
                    // �߰� ����
                    nmAgent.acceleration = Random.Range(7f, 10f);
                }

                // ���� �ӵ��� ���� �ִϸ��̼� �ӵ� ����
                if (anim != null)
                {
                    float animSpeedMultiplier = newSpeed / 4.5f; // �⺻ �ӵ� ���
                    anim.SetFloat("SpeedMultiplier", animSpeedMultiplier);
                }

                // ���� ���� ���� ���� ���� (�ʹ� ����� ������ ����)
                attackCooldown = Random.Range(2.5f, 4.0f);
                aimTime = attackCooldown * 0.6f;
                attackTime = attackCooldown * 0.8f;
            }

            // ���� �ӵ� ������� ���� �ð� ���
            float waitTime = speedChangeInterval + Random.Range(-speedChangeVariation, speedChangeVariation);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void InitializeMovementBehavior()
    {
        // ID�� ���� �ʱ� �÷�ŷ ���� ���� (�� �׸��ڰ� �ٸ� ���⿡�� ����)
        float initialAngle = shadowID * (360f / 6f) + Random.Range(-20f, 20f);

        // ���� ������ ���� ����
        if (nmAgent != null)
        {
            // �� �׸����� ���ݿ� ���� �ʱ� �ӵ��� ���ӵ� �ణ �ٸ��� ����
            float speedVariation = Random.Range(0.8f, 1.2f);
            nmAgent.speed = chaseSpeed * speedVariation;
            nmAgent.acceleration = nmAgent.acceleration * speedVariation;

            // ��� ���� ������ �ణ �ٸ��� ����
            nmAgent.avoidancePriority = Random.Range(30, 70); // �켱������ �ٸ��� ����
        }

        // �׸��� ������ ������ ID�� ���� �ٸ��� ����
        StartCoroutine(ApplyMovementPattern(initialAngle));
    }

    private IEnumerator ApplyMovementPattern(float initialAngle)
    {
        // �ʱ� ������ (��� �׸��ڰ� ���ÿ� �������� �ʵ���)
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));

        while (!isDying)
        {
            // �÷��̾ ���� ���� ����
            if (target != null && nmAgent != null && nmAgent.enabled)
            {
                // ���� �ٸ� �׸��ڵ���� �Ÿ� Ȯ��
                bool tooCloseToOthers = IsCloseToOtherShadows();

                // ���ο� ��ġ ��� (�÷��̾� �ֺ����� �л�� ��ġ)
                Vector3 newPosition;

                if (tooCloseToOthers)
                {
                    // �ٸ� �׸��ڿ��� �־����� �������� �̵�
                    newPosition = CalculatePositionAwayFromOthers();
                }
                else
                {
                    // �׸��� ID�� ���� �÷��̾� �ֺ��� �ٸ� ��ġ�� ����
                    float angle = initialAngle + Time.time * 15f * (shadowID % 2 == 0 ? 1 : -1); // �ð��� ���� ���� ��ȭ, ¦��/Ȧ�� ID�� ���� �ݴ��
                    newPosition = CalculateFlankingPosition(angle);
                }

                // NavMesh�� ��ȿ�� ��ġ���� Ȯ��
                NavMeshHit hit;
                if (NavMesh.SamplePosition(newPosition, out hit, 5f, NavMesh.AllAreas))
                {
                    // �׸��ڸ��� �ణ �ٸ� ������ ����
                    nmAgent.SetDestination(hit.position);
                }
            }

            // ���� ��ġ ������Ʈ���� ���
            yield return new WaitForSeconds(positionUpdateInterval + Random.Range(-0.5f, 0.5f));
        }
    }

    private bool IsCloseToOtherShadows()
    {
        // �ٸ� ������ ���������� �Ÿ� Ȯ��
        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        foreach (var shadow in shadows)
        {
            if (shadow != this && !shadow.isDying)
            {
                float distance = Vector3.Distance(transform.position, shadow.transform.position);
                if (distance < minDistanceFromOthers)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3 CalculatePositionAwayFromOthers()
    {
        if (target == null) return transform.position;

        // �ٸ� �׸��ڵ�κ��� �־����� ���� ���
        Vector3 awayDirection = Vector3.zero;
        int shadowCount = 0;

        DigitalShadow[] shadows = FindObjectsOfType<DigitalShadow>();
        foreach (var shadow in shadows)
        {
            if (shadow != this && !shadow.isDying)
            {
                Vector3 directionFromShadow = transform.position - shadow.transform.position;
                float distance = directionFromShadow.magnitude;

                if (distance < minDistanceFromOthers * 2f)
                {
                    // �Ÿ��� �������� �� ���ϰ� �о
                    float repulsionStrength = 1.0f - (distance / (minDistanceFromOthers * 2f));
                    awayDirection += directionFromShadow.normalized * repulsionStrength;
                    shadowCount++;
                }
            }
        }

        if (shadowCount > 0)
        {
            awayDirection /= shadowCount; // ��� ����

            // �÷��̾� ���⵵ ��� (�÷��̾�Լ� �ʹ� �־����� �ʵ���)
            Vector3 playerDirection = target.position - transform.position;
            float distanceToPlayer = playerDirection.magnitude;

            // �÷��̾���� �Ÿ��� ���� ����ġ ����
            float playerWeight = Mathf.Clamp01((distanceToPlayer - minDistanceFromPlayer) / (maxDistanceFromPlayer - minDistanceFromPlayer));
            Vector3 finalDirection = Vector3.Lerp(awayDirection.normalized, playerDirection.normalized, playerWeight * 0.5f);

            // ���� ��ġ ���
            float targetDistance = Mathf.Lerp(minDistanceFromPlayer, maxDistanceFromPlayer, Random.value);
            return transform.position + finalDirection * targetDistance;
        }

        // �ٸ� �׸��ڰ� ������ �÷�ŷ ��ġ ���
        return CalculateFlankingPosition(shadowID * 60f);
    }

    private Vector3 CalculateFlankingPosition(float angle){
        if (target == null) return transform.position;

        // �÷��̾� �ֺ��� ���� ��ġ ��� (�����̳� �Ĺ濡�� �����ϵ���)
        Vector3 playerPosition = target.position;
        Vector3 forward = target.forward;

        // �÷��̾� ���� ���͸� �������� ���� ���
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        Vector3 direction = rotation * forward;

        // �Ÿ� ����ȭ (min~max ����)
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        Vector3 targetPosition = playerPosition + direction * distance;

        return targetPosition;
    }

    protected override void Start()
    {
        base.Start();

        // RangedMonster�� Start() ȣ�� �� �߰� ����
        attackCooldown = 3f; // �� ���� �����ϵ��� ��ٿ� ����
        chaseSpeed = 4.5f; // �ణ �� ������ �̵�
        attackRange = 10f; // ���� ���� ����

        // ���� ���� �� ��ٿ� ����ȭ (��ü���� �ణ �ٸ���)
        attackRange *= Random.Range(0.8f, 1.2f);
        attackCooldown *= Random.Range(0.9f, 1.1f);
        aimTime = attackCooldown * 0.6f;
        attackTime = attackCooldown * 0.8f;

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

        // �߾� ���Ͽ� �п� �����̸� Ư�� ȿ�� �߰�
        if (isLastStandFragment)
        {
            // �� ���� ���� ȿ�� ����
            GameObject explosion = new GameObject("LastStandExplosion");
            explosion.transform.position = transform.position;

            // �߰� ��ƼŬ �ý��� �Ǵ� ����Ʈ �߰� ����
            ParticleSystem explosionPS = explosion.AddComponent<ParticleSystem>();
            var main = explosionPS.main;
            main.startColor = new Color(1.0f, 0.2f, 0.2f);
            main.startSize = 5.0f;
            main.startLifetime = 2.0f;

            // ���� �ð� �� ���� ������Ʈ ����
            Destroy(explosion, 3.0f);

            // �ֺ� �ٸ� ���Ϳ��� �������� �� ���� ����
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5.0f);
            foreach (var collider in colliders)
            {
                // �ٸ� ���Ϳ��� ������ ���� ����
                RangedMonster monster = collider.GetComponent<RangedMonster>();
                if (monster != null && monster != this)
                {
                    monster.TakeDamage(20f, true);
                }
            }
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
                if (material.HasProperty("_FlashIntensity"))
                {
                    material.SetFloat("_FlashIntensity", 1f);
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_FlashIntensity"))
                {
                    material.SetFloat("_FlashIntensity", 0f);
                }
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

        // �߾� ��忡���� �۸�ġ ȿ�� �⺻���� �� ����
        if (isLastStandFragment)
        {
            glitchIntensity = Mathf.Max(glitchIntensity, 0.5f);
        }

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

// �÷��̾� �ֺ��� ���� ��ġ ��� (������