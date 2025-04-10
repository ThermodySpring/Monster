using UnityEngine;
using System.Collections;
using UnityEngine.U2D; // ��������Ʈ ��Ʋ�� ���ӽ����̽� �߰�

public class BinaryDeathEffect : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem particleSystem;
    public Material binaryParticleMaterial; // ���̳ʸ� ���̴� ��Ƽ����

    [Header("Binary Particle Settings")]
    public SpriteAtlas binaryAtlas; // ��������Ʈ ��Ʋ��
    public string zeroSpriteName = "zero"; // ��Ʋ�� �� 0 ��������Ʈ �̸�
    public string oneSpriteName = "one"; // ��Ʋ�� �� 1 ��������Ʈ �̸�
    [Range(0f, 1f)]
    public float zeroToOneRatio = 0.5f; // 0�� 1�� ����

    [Header("Effect Settings")]
    public Color primaryColor = new Color(0f, 1f, 0.5f, 1f);
    public Color secondaryColor = new Color(0f, 0.6f, 1f, 1f);
    [Range(0.1f, 10f)]
    public float emissionRate = 50f;
    [Range(0.5f, 5f)]
    public float effectRadius = 1f;
    [Range(1f, 5f)]
    public float effectDuration = 2f;
    [Range(1f, 10f)]
    public float riseSpeed = 3f;
    [Range(0f, 3f)]
    public float glowIntensity = 1.5f;

    // ĳ�̵� ��������Ʈ
    private Sprite zeroSprite;
    private Sprite oneSprite;

    private void Awake()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        // ��Ʋ�󽺿��� ��������Ʈ �ε�
        LoadSpritesFromAtlas();

        ConfigureParticleSystem();
    }

    void LoadSpritesFromAtlas()
    {
        if (binaryAtlas == null)
        {
            Debug.LogError("��������Ʈ ��Ʋ�󽺰� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ��Ʋ�󽺿��� ��������Ʈ �ε�
        zeroSprite = binaryAtlas.GetSprite(zeroSpriteName);
        oneSprite = binaryAtlas.GetSprite(oneSpriteName);

        // ��������Ʈ �ε� Ȯ��
        if (zeroSprite == null)
            Debug.LogError($"'{zeroSpriteName}' �̸��� ��������Ʈ�� ��Ʋ�󽺿��� ã�� �� �����ϴ�.");
        if (oneSprite == null)
            Debug.LogError($"'{oneSpriteName}' �̸��� ��������Ʈ�� ��Ʋ�󽺿��� ã�� �� �����ϴ�.");
    }

    void ConfigureParticleSystem()
    {
        // ��Ʋ�󽺿��� ��������Ʈ�� �ε����� ���ߴٸ� �������� ����
        if (zeroSprite == null || oneSprite == null)
            return;

        // ���� ��� ����
        var main = particleSystem.main;
        main.startLifetime = effectDuration;
        main.startSpeed = riseSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        // �̹̼� ��� ����
        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)(emissionRate * 0.5f))
        });

        // ������ ��� ����
        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = effectRadius;
        shape.rotation = new Vector3(-90f, 0f, 0f); // ������ ���ϵ��� ȸ��


        // �ؽ��� ��Ʈ �ִϸ��̼� ����
        var textureSheet = particleSystem.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Sprites;

        // ���� ��������Ʈ ���� �� ��Ʋ�󽺿��� �ε��� ��������Ʈ �߰�
        textureSheet.AddSprite(zeroSprite);
        textureSheet.AddSprite(oneSprite);

        // ũ�� ��ȭ ����
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.7f),
            new Keyframe(0.2f, 1f),
            new Keyframe(0.7f, 1f),
            new Keyframe(1f, 0f)
        ));

        // ������ ���� (���̴� ��Ƽ���� ����)
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (binaryParticleMaterial != null)
        {
            renderer.material = binaryParticleMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // ���̴� ������Ƽ ������Ʈ
            binaryParticleMaterial.SetColor("_PrimaryColor", primaryColor);
            binaryParticleMaterial.SetColor("_SecondaryColor", secondaryColor);
            binaryParticleMaterial.SetFloat("_GlowIntensity", glowIntensity);
            binaryParticleMaterial.SetFloat("_RiseSpeed", riseSpeed);
        }
    }

    public void TriggerDeathEffect(Vector3 position)
    {
        // ��ƼŬ �ý��� ��ġ ���� �� ���
        transform.position = position;
        particleSystem.Clear();

        // 0�� 1 ������ ���� ��ƼŬ ��������Ʈ �ε��� ����
        SetParticleSprites();

        particleSystem.Play();

        // ȿ�� �Ϸ� �� ���� (������)
        StartCoroutine(CleanupAfterEffect());
    }

    private void SetParticleSprites()
    {
        // ������ ��ƼŬ�� 0 �Ǵ� 1 ��������Ʈ �ε��� �Ҵ�
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
        int count = particleSystem.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // zeroToOneRatio�� ���� 0 �Ǵ� 1 ���� (0: ����, 1: ��)
            bool useOne = Random.value < zeroToOneRatio;
            particles[i].randomSeed = (uint)(useOne ? 1 : 0);
        }

        if (count > 0)
        {
            particleSystem.SetParticles(particles, count);
        }
    }

    private IEnumerator CleanupAfterEffect()
    {
        yield return new WaitForSeconds(effectDuration + 1f);
        // �ڵ� �ı��� ���ϸ� �Ʒ� �ּ� ����
        // Destroy(gameObject);
    }

    // Unity �ν����Ϳ��� �� ���� �� ��ƼŬ �ý��� ������Ʈ
    private void OnValidate()
    {
        if (Application.isPlaying && particleSystem != null)
        {
            // ��Ʋ�󽺿��� ��������Ʈ �ٽ� �ε�
            LoadSpritesFromAtlas();
            ConfigureParticleSystem();
        }
    }
}