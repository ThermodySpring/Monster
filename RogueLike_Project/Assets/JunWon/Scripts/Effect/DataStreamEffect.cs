using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataStreamEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem particleSystem;
    public Material streamParticleMaterial;

    [Header("Stream Settings")]
    [Range(1, 20)]
    public int streamCount = 5;  // ������ ��Ʈ�� ����
    [Range(5, 100)]
    public int particlesPerStream = 20;  // �ϳ��� ��Ʈ���� ��ƼŬ ��
    [Range(0.01f, 1f)]
    public float streamWidth = 0.05f;  // ��Ʈ�� ��
    [Range(0.1f, 10f)]
    public float streamLength = 3f;  // ��Ʈ�� ����

    [Header("Target Points")]
    public Transform sourcePoint;  // ������
    public List<Transform> targetPoints = new List<Transform>();  // ��ǥ����

    [Header("Appearance")]
    public Color primaryColor = new Color(0f, 1f, 0.5f, 1f);
    public Color secondaryColor = new Color(0f, 0.6f, 1f, 1f);
    public Color glowColor = new Color(0f, 1f, 0f, 1f);
    [Range(0.5f, 10f)]
    public float streamSpeed = 4f;
    [Range(0f, 3f)]
    public float glowIntensity = 1f;
    [Range(0f, 0.3f)]
    public float noiseAmount = 0.05f;  // ��Ʈ�� �䵿 ����

    [Header("Timing")]
    [Range(0.5f, 10f)]
    public float effectDuration = 3f;  // ȿ�� ���� �ð�
    [Range(0.1f, 2f)]
    public float streamStartInterval = 0.2f;  // ��Ʈ�� ���� ����

    private List<Vector3> streamDirections = new List<Vector3>();
    private bool isEffectPlaying = false;

    private void Awake()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if (sourcePoint == null)
            sourcePoint = transform;
    }

    void Start()
    {
        ConfigureParticleSystem();
    }

    void Update()
    {
        // ���̴� �Ķ���� �ǽð� ������Ʈ
        if (isEffectPlaying && streamParticleMaterial != null)
        {
            UpdateShaderParameters();
        }
    }

    void ConfigureParticleSystem()
    {
        if (particleSystem == null) return;

        // �⺻ ��� ����
        var main = particleSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = streamCount * particlesPerStream;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = effectDuration;
        main.startSize = streamWidth;
        main.startSpeed = 0f;  // �ӵ��� ���̴����� ó��
        main.startColor = Color.white;  // ������ ���̴����� ó��

        // �̹̼� ��� ��Ȱ��ȭ (�������� ��ƼŬ �߻�)
        var emission = particleSystem.emission;
        emission.enabled = false;

        // ������ ��� ��Ȱ��ȭ
        var shape = particleSystem.shape;
        shape.enabled = false;

        // ������ ����
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (streamParticleMaterial != null)
        {
            renderer.material = streamParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.minParticleSize = 0.01f;
            renderer.maxParticleSize = 1f;
            renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
        }

        // �ؽ�ó ��Ʈ �ִϸ��̼� ��� ���� (������ Ÿ�� ǥ����)
        var textureSheet = particleSystem.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.numTilesX = 1;
        textureSheet.numTilesY = 1;
    }

    void UpdateShaderParameters()
    {
        if (streamParticleMaterial != null)
        {
            streamParticleMaterial.SetColor("_PrimaryColor", primaryColor);
            streamParticleMaterial.SetColor("_SecondaryColor", secondaryColor);
            streamParticleMaterial.SetColor("_GlowColor", glowColor);
            streamParticleMaterial.SetFloat("_StreamWidth", streamWidth);
            streamParticleMaterial.SetFloat("_StreamSpeed", streamSpeed);
            streamParticleMaterial.SetFloat("_GlowIntensity", glowIntensity);
            streamParticleMaterial.SetFloat("_NoiseAmount", noiseAmount);
        }
    }

    public void TriggerStreamEffect()
    {
        if (isEffectPlaying) return;

        if (targetPoints.Count == 0)
        {
            Debug.LogWarning("No target points specified for data stream effect.");
            return;
        }

        // ���� ��ƼŬ �ʱ�ȭ
        particleSystem.Clear();

        // ��Ʈ�� ���� ���
        CalculateStreamDirections();

        // ��Ʈ�� ����
        StartCoroutine(StartStreamEffect());
    }

    void CalculateStreamDirections()
    {
        streamDirections.Clear();

        // �ҽ� ��ġ
        Vector3 source = sourcePoint.position;

        // �� Ÿ�� ����Ʈ�� ���� ���� ���
        foreach (Transform target in targetPoints)
        {
            if (target != null)
            {
                Vector3 direction = target.position - source;
                streamDirections.Add(direction.normalized);
            }
        }

        // Ÿ���� ������ ���, ���� ���� �߰�
        while (streamDirections.Count < streamCount)
        {
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f  // 2D ȿ���� ��� Z���� 0���� ����
            ).normalized;

            streamDirections.Add(randomDir);
        }
    }

    IEnumerator StartStreamEffect()
    {
        isEffectPlaying = true;

        // �� ��Ʈ���� ������ �������� ����
        for (int streamIndex = 0; streamIndex < streamCount; streamIndex++)
        {
            CreateStream(streamIndex);
            yield return new WaitForSeconds(streamStartInterval);
        }

        // ȿ�� ���� ���
        yield return new WaitForSeconds(effectDuration);

        isEffectPlaying = false;
    }

    void CreateStream(int streamIndex)
    {
        if (streamIndex >= streamDirections.Count) return;

        // ��Ʈ���� ���� ����
        Vector3 direction = streamDirections[streamIndex];

        // ���� ���͸� 0-1 ���� ��Į�� ������ ��ȯ (���� ��ȯ)
        float angle = Mathf.Atan2(direction.y, direction.x) / (2f * Mathf.PI);
        if (angle < 0) angle += 1f; // 0-1 ������ ����

        // �� ��Ʈ���� ��ƼŬ ����
        var particles = new ParticleSystem.Particle[particlesPerStream];

        for (int i = 0; i < particlesPerStream; i++)
        {
            // ��Ʈ�� �� ��ġ�� ���� ��ġ (0~1 ���� ���൵)
            float progress = (float)i / particlesPerStream;

            // ��ƼŬ ��ġ ��� (���������� ���� * ���� * ���൵)
            Vector3 position = sourcePoint.position + direction * streamLength * progress;

            // �⺻ ��ƼŬ ����
            particles[i].position = position;
            particles[i].startColor = new Color(angle, streamIndex / (float)streamCount,
                                               Random.Range(0.8f, 1.2f), 1.0f);
            particles[i].startSize = streamWidth;
            particles[i].remainingLifetime = effectDuration - (progress * streamStartInterval * streamCount);

            // ��Ʈ�� ������ �ӵ��� ���� (���̴����� ��ġ ����� ���� ����)
            particles[i].velocity = direction * streamSpeed;

            // ���� ȸ�� ���� (�ð��� �پ缺��)
            particles[i].rotation = Random.Range(0f, 360f);

            // ũ�� ��ȭ ���� (ù ��ƼŬ�� �۰�, �߰��� ũ��, �������� �ٽ� �۰�)
            float sizeMultiplier = 0.5f + Mathf.Sin(progress * Mathf.PI) * 0.5f;
            particles[i].startSize *= sizeMultiplier;
        }

        // ��ƼŬ �ý��ۿ� �߰�
        particleSystem.SetParticles(particles, particles.Length);
    }

    // Unity �ν����Ϳ��� �� ���� ��
    private void OnValidate()
    {
        if (Application.isPlaying && particleSystem != null)
        {
            ConfigureParticleSystem();
            UpdateShaderParameters();
        }
    }
}