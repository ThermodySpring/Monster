using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentBehavior : MonoBehaviour
{
    private float lifetime;
    private float fadeStartTime;
    private bool shouldFade;
    private Renderer fragmentRenderer;
    private Material fragmentMaterial;
    private Color originalColor;
    private float spawnTime;

    [Header("�ٿ ȿ��")]
    [SerializeField] private float bounceReduction = 0.7f; // �ٿ�� ������ �ӵ� ����
    [SerializeField] private int maxBounces = 3; // �ִ� �ٿ Ƚ��
    private int bounceCount = 0;

    [Header("ȸ�� ����")]
    [SerializeField] private float rotationDecay = 0.95f; // ȸ�� ����

    public void Initialize(float lifetime, float fadeStartTime, bool shouldFade)
    {
        this.lifetime = lifetime;
        this.fadeStartTime = fadeStartTime;
        this.shouldFade = shouldFade;
        this.spawnTime = Time.time;

        fragmentRenderer = GetComponent<Renderer>();
        if (fragmentRenderer != null)
        {
            fragmentMaterial = fragmentRenderer.material;
            originalColor = fragmentMaterial.color;
        }
    }

    private void Update()
    {
        // ȸ�� ����
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity *= rotationDecay;
        }

        // ���� üũ
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �ٴڿ� ����� �� �ٿ ȿ��
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                // �� �̻� �ٿ���� �ʰ� ����
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity *= 0.1f;
                    rb.angularVelocity *= 0.1f;
                }
            }
            else
            {
                // �ٿ ȿ�� ����
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity *= bounceReduction;
                }
            }

            // �ٿ ��ƼŬ ȿ�� (�ɼ�)
            CreateBounceEffect(collision.contacts[0].point);
        }
    }

    private void CreateBounceEffect(Vector3 position)
    {
        // ������ ���� ȿ��
        GameObject dustEffect = new GameObject("DustEffect");
        dustEffect.transform.position = position;

        ParticleSystem particles = dustEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = Color.gray;
        main.maxParticles = 10;

        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 10)
        });

        Destroy(dustEffect, 2f);
    }

    public void StartFadeOut(float fadeDuration)
    {
        if (shouldFade && fragmentMaterial != null)
        {
            StartCoroutine(FadeOutCoroutine(fadeDuration));
        }
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / duration);

            Color newColor = originalColor;
            newColor.a = alpha;
            fragmentMaterial.color = newColor;

            yield return null;
        }
    }
}
