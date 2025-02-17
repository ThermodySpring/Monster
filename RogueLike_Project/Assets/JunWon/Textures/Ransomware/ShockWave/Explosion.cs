using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionDuration = 1.5f;
    [SerializeField] private float maxExplosionRadius = 10.0f;
    [SerializeField] private float baseDistortion = 0.2f;
    [SerializeField] private AnimationCurve explosionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve distortionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Shockwave Settings")]
    [SerializeField] private float shockwaveSpeed = 5f;
    [SerializeField] private float shockwaveThickness = 0.5f;
    [SerializeField] private float shockwaveIntensity = 2f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private GameObject shockwaveRing;
    [SerializeField] private Light explosionLight;

    private Material explosionMaterial;
    private Material shockwaveMaterial;
    private Vector3 originalScale;
    private float startTime;
    private bool hasTriggeredDamage;

    private void Start()
    {
        InitializeComponents();
        SetupExplosion();
        StartCoroutine(ExplosionSequence());
    }

    private void InitializeComponents()
    {
        // ���� ���� ����Ʈ �ʱ�ȭ
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            explosionMaterial = renderer.material;
        }
        else
        {
            Debug.LogError("Renderer component not found on explosion prefab!");
            return;
        }

        // ��ũ���̺� �ʱ�ȭ
        if (shockwaveRing != null)
        {
            var shockwaveRenderer = shockwaveRing.GetComponent<Renderer>();
            if (shockwaveRenderer != null)
            {
                shockwaveMaterial = shockwaveRenderer.material;
            }
        }

        originalScale = transform.localScale;
    }

    private void SetupExplosion()
    {
        startTime = Time.time;
        hasTriggeredDamage = false;

        // ���� ��ƼŬ �ý��� ����
        if (explosionParticles != null)
        {
            var main = explosionParticles.main;
            main.duration = explosionDuration;
            explosionParticles.Play();
        }

        // ���� ����Ʈ ����
        if (explosionLight != null)
        {
            StartCoroutine(ExplosionLightEffect());
        }
    }

    private IEnumerator ExplosionSequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < explosionDuration)
        {
            float normalizedTime = elapsedTime / explosionDuration;

            // ���� �ݰ� ������Ʈ
            float explosionProgress = explosionCurve.Evaluate(normalizedTime);
            float currentRadius = maxExplosionRadius * explosionProgress;

            // �ְ� ȿ�� ������Ʈ
            float distortionProgress = distortionCurve.Evaluate(normalizedTime);
            float currentDistortion = baseDistortion * distortionProgress;

            // ��Ƽ���� ������Ƽ ������Ʈ
            if (explosionMaterial != null)
            {
                explosionMaterial.SetFloat("_ExplosionRadius", currentRadius);
                explosionMaterial.SetFloat("_Distortion", currentDistortion);
                transform.localScale = originalScale * (1.0f + currentRadius);
            }

            // ��ũ���̺� ������Ʈ
            UpdateShockwave(normalizedTime);

            // ������ Ʈ���� (�� ����)
            if (!hasTriggeredDamage && normalizedTime >= 0.1f)
            {
                TriggerExplosionDamage();
                hasTriggeredDamage = true;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� ���� ó��
        OnExplosionComplete();
    }

    private void UpdateShockwave(float progress)
    {
        if (shockwaveMaterial != null)
        {
            float shockwaveRadius = progress * shockwaveSpeed;
            shockwaveMaterial.SetFloat("_Radius", shockwaveRadius);
            shockwaveMaterial.SetFloat("_Thickness", shockwaveThickness * (1 - progress));
            shockwaveMaterial.SetFloat("_Intensity", shockwaveIntensity * (1 - progress));
        }
    }

    private IEnumerator ExplosionLightEffect()
    {
        float intensity = explosionLight.intensity;
        float range = explosionLight.range;

        while (explosionLight.intensity > 0)
        {
            float normalizedTime = (Time.time - startTime) / explosionDuration;
            explosionLight.intensity = Mathf.Lerp(intensity, 0, normalizedTime);
            explosionLight.range = Mathf.Lerp(range, 0, normalizedTime);
            yield return null;
        }
    }

    private void TriggerExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // �Ÿ��� ���� ������ ���
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            float damageRatio = 1 - (distance / maxExplosionRadius);
            if (damageRatio > 0)
            {
                var damageable = hitCollider.GetComponent<PlayerStatus>();
                if (damageable != null)
                {
                    float damage = 100 * damageRatio; // �⺻ ������ �� ���� ����
                    damageable.DecreaseHealth(damage);
                }
            }
        }
    }

    private void OnExplosionComplete()
    {
        // ��ƼŬ �ý��� ����
        if (explosionParticles != null)
        {
            explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // ������Ʈ ����
        Destroy(gameObject, 0.5f); // ��ƼŬ�� ������ ����� ������ �ణ�� ������
    }

}
