using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalStripeDissolveEffect : MonoBehaviour
{
    [Header("������ ����")]
    [SerializeField] private Material stripeDissolveShader; // ������ DigitalStripeDissolve ���̴� ����
    [SerializeField] private float dissolveDuration = 5.0f; // ������ ȿ�� ���� �ð�

    [Header("��Ʈ������ ȿ�� ����")]
    [SerializeField] private float stripeWidthStart = 10f;
    [SerializeField] private float stripeWidthEnd = 30f;
    [SerializeField] private float stripeSpeedStart = 2f;
    [SerializeField] private float stripeSpeedEnd = 4f;

    [Header("�۸�ġ ȿ�� ����")]
    [SerializeField] private float glitchDelayTime = 0.3f; // �۸�ġ ȿ�� ���� ���� �ð�
    [SerializeField] private float glitchIntensityMax = 0.8f;

    [Header("������ ���� ����")]
    [SerializeField] private Vector3 dissolveDirection = Vector3.up; // �⺻ ����: ����

    [Header("���� ����")]
    [SerializeField] private Color dissolveColor = new Color(0f, 0.7f, 1f);

    [Header("���� ȿ��")]
    [SerializeField] private AudioClip dissolveSound;
    [SerializeField] private AudioClip glitchSound;

    private AudioSource audioSource;
    [SerializeField] private List<Material> dissolveMaterials = new List<Material>();
    [SerializeField] private Dictionary<SkinnedMeshRenderer, Material[]> originalSkinnedMaterials = new Dictionary<SkinnedMeshRenderer, Material[]>();
    [SerializeField] private Dictionary<Renderer, Material[]> originalRendererMaterials = new Dictionary<Renderer, Material[]>();

    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private Renderer[] regularRenderers;
    private MonsterBase monster;
    private int randomSeed;
    private Vector3 objectCenterPosition;
    private float objectHeight;

    // ���� ��� ȿ�� ���� ���� �޼���
    public static void ApplyDeathEffect(MonsterBase targetMonster)
    {
        //Debug.Log("���� ���� ���� ����");
        // �̹� ȿ���� ����Ǿ����� Ȯ��
        if (targetMonster.GetComponent<DigitalStripeDissolveEffect>() == null)
            return;

        Debug.Log(" ��� ����");

        // ȿ�� ������Ʈ �߰�
       DigitalStripeDissolveEffect effect = targetMonster.GetComponent<DigitalStripeDissolveEffect>();
        effect.Initialize();
        effect.StartDeathSequence();
    }

    private void Initialize()
    {
        // ����� �ҽ� �߰�
        audioSource = gameObject.AddComponent<AudioSource>();

        // ���� �õ� ���� (��� ���� ��ü�� ������ ���� ���� ������ ����)
        randomSeed = Random.Range(1, 10000);

        // SkinnedMeshRenderer�� �Ϲ� Renderer �и� ����
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // SkinnedMeshRenderer�� �ƴ� �Ϲ� Renderer�� ����
        List<Renderer> regularRenderersList = new List<Renderer>();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (!(renderer is SkinnedMeshRenderer))
            {
                regularRenderersList.Add(renderer);
            }
        }
        regularRenderers = regularRenderersList.ToArray();

        // ���� ������Ʈ�� �ٿ�� �ڽ� ���� ���ϱ�
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;

        // ��� �������� �����ϴ� �ٿ�� �ڽ� ���
        foreach (Renderer renderer in allRenderers)
        {
            if (!boundsInitialized)
            {
                combinedBounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
        }

        // ������Ʈ �߽����� ���� ���
        objectCenterPosition = combinedBounds.center;
        objectHeight = combinedBounds.size.y;

        // ������ ���̴� �ν��Ͻ�ȭ
        if (stripeDissolveShader == null)
        {
            stripeDissolveShader = new Material(Shader.Find("Custom/DigitalStripeDissolve"));
        }
        else
        {
        }

        // ���� ������ �´� ���� ����
        AdjustColorForMonsterType();

        // SkinnedMeshRenderer ó��
        ProcessSkinnedMeshRenderers();

        // �Ϲ� Renderer ó��
        ProcessRegularRenderers();
    }

    private void ProcessSkinnedMeshRenderers()
    {
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            // ���� ���� ����
            Material[] originalMaterials = renderer.materials;
            originalSkinnedMaterials[renderer] = originalMaterials;

            // �� ���� ����
            Material[] newMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material dissolveMat = new Material(stripeDissolveShader);

                // ���� �ؽ�ó�� ���� ����
                if (originalMaterials[i].HasProperty("_MainTex"))
                {
                    dissolveMat.SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
                }

                if (originalMaterials[i].HasProperty("_Color"))
                {
                    dissolveMat.SetColor("_Color", originalMaterials[i].GetColor("_Color"));
                }

                // ���� ���̴� �Ӽ� ����
                SetShaderCommonProperties(dissolveMat);

                // SkinnedMeshRenderer�� Ưȭ�� ����
                ConfigureShaderForSkinnedMesh(dissolveMat, renderer);

                newMaterials[i] = dissolveMat;
                dissolveMaterials.Add(dissolveMat);
            }

            // �� ���������� ������ �������� �ʰ� ���常 ��
        }
    }

    private void ProcessRegularRenderers()
    {
        foreach (Renderer renderer in regularRenderers)
        {
            // ���� ���� ����
            Material[] originalMaterials = renderer.materials;
            originalRendererMaterials[renderer] = originalMaterials;

            // �� ���� ����
            Material[] newMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material dissolveMat = new Material(stripeDissolveShader);

                // ���� �ؽ�ó�� ���� ����
                if (originalMaterials[i].HasProperty("_MainTex"))
                {
                    dissolveMat.SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
                }

                if (originalMaterials[i].HasProperty("_Color"))
                {
                    dissolveMat.SetColor("_Color", originalMaterials[i].GetColor("_Color"));
                }

                // ���� ���̴� �Ӽ� ����
                SetShaderCommonProperties(dissolveMat);

                // �Ϲ� �������� Ưȭ�� ����
                ConfigureShaderForRenderer(dissolveMat, renderer);

                newMaterials[i] = dissolveMat;
                dissolveMaterials.Add(dissolveMat);
            }

            // �� ���������� ������ �������� �ʰ� ���常 ��
        }
    }

    private void SetShaderCommonProperties(Material material)
    {
        // �⺻ ���̴� �Ӽ� ����
        material.SetFloat("_DissolveAmount", 0f);
        material.SetFloat("_StripeWidth", stripeWidthStart);
        material.SetFloat("_StripeSpeed", stripeSpeedStart);
        material.SetFloat("_GlitchIntensity", 0.3f);
        material.SetFloat("_StripeIntensity", 0.7f);
        material.SetColor("_GlowColor", dissolveColor);

        // �߰��� �Ӽ� ����
        material.SetFloat("_RandomSeed", randomSeed); // ���� ���� �õ� ����
        material.SetFloat("_UseWorldCoords", 1f); // ���� ��ǥ ���

        // ������ ���� ����ȭ�Ͽ� ����
        material.SetVector("_DissolveDirection", dissolveDirection.normalized);
    }

    private void ConfigureShaderForSkinnedMesh(Material material, SkinnedMeshRenderer renderer)
    {
        // SkinnedMeshRenderer�� ����� ��ġ�� ���� ����
        float relativeHeight = (renderer.bounds.center.y - (objectCenterPosition.y - objectHeight / 2)) / objectHeight;

        // ���� Y ������ ���� (��� ���� ��ü�� ���ÿ� ������ǵ���)
        material.SetFloat("_WorldYOffset", relativeHeight);
    }

    private void ConfigureShaderForRenderer(Material material, Renderer renderer)
    {
        // �Ϲ� �������� ����� ��ġ�� ���� ����
        float relativeHeight = (renderer.bounds.center.y - (objectCenterPosition.y - objectHeight / 2)) / objectHeight;

        // ���� Y ������ ���� (��� ���� ��ü�� ���ÿ� ������ǵ���)
        material.SetFloat("_WorldYOffset", relativeHeight);
    }

    private void AdjustColorForMonsterType()
    {

        dissolveColor = new Color(0.2f, 0.7f, 1f); // �Ķ���
        
    }

    // ������ ���� ���� �޼��� (�ܺο��� ȣ�� ����)
    public void SetDissolveDirection(Vector3 direction)
    {
        dissolveDirection = direction.normalized;

        // �̹� ������ �����鿡�� ����
        foreach (Material mat in dissolveMaterials)
        {
            mat.SetVector("_DissolveDirection", dissolveDirection);
        }
    }

    // ������ ���ӽð� ���� �޼��� (�ܺο��� ȣ�� ����)
    public void SetDissolveDuration(float duration)
    {
        if (duration > 0f)
        {
            dissolveDuration = duration;
        }
    }

    public void StartDeathSequence()
    {
        StartCoroutine(DissolveRoutine());

        // ���� ȿ�� ���
        PlayDissolveSound();
    }


    private IEnumerator DissolveRoutine()
    {
        // ��� SkinnedMeshRenderer�� ������ ���̴� ����
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            int materialCount = renderer.materials.Length;
            Material[] newMaterials = new Material[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                // dissolveMaterials ����Ʈ���� �ε��� ���
                int materialIndex = dissolveMaterials.FindIndex(m =>
                    m.GetTexture("_MainTex") == renderer.materials[i].GetTexture("_MainTex") &&
                    m.GetColor("_Color") == renderer.materials[i].GetColor("_Color"));

                if (materialIndex >= 0)
                {
                    newMaterials[i] = dissolveMaterials[materialIndex];
                }
                else
                {
                    // ã�� ������ ��� �⺻ ������ ���� ���
                    newMaterials[i] = new Material(stripeDissolveShader);
                    SetShaderCommonProperties(newMaterials[i]);
                    dissolveMaterials.Add(newMaterials[i]);
                }
            }

            renderer.materials = newMaterials;
        }

        // ��� �Ϲ� Renderer�� ������ ���̴� ����
        foreach (Renderer renderer in regularRenderers)
        {
            int materialCount = renderer.materials.Length;
            Material[] newMaterials = new Material[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                // dissolveMaterials ����Ʈ���� �ε��� ���
                int materialIndex = 0;
                // �ε��� ã�� ����...

                if (materialIndex < dissolveMaterials.Count)
                {
                    newMaterials[i] = dissolveMaterials[materialIndex];
                    materialIndex++;
                }
                else
                {
                    // �⺻ ������ ���� ���
                    newMaterials[i] = new Material(stripeDissolveShader);
                    SetShaderCommonProperties(newMaterials[i]);
                    dissolveMaterials.Add(newMaterials[i]);
                }
            }

            renderer.materials = newMaterials;
        }

        float elapsed = 0f;
        bool glitchStarted = false;

        // ������ �ִϸ��̼�
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / dissolveDuration;

            // ������ ���൵ ������Ʈ
            float dissolveAmount = Mathf.Lerp(0f, 1f, progress);

            // ��Ʈ������ ȿ�� ������ ��ȭ
            float stripeWidth = Mathf.Lerp(stripeWidthStart, stripeWidthEnd, progress);
            float stripeSpeed = Mathf.Lerp(stripeSpeedStart, stripeSpeedEnd, progress);

            // �۸�ġ ȿ���� ���� �ð� �� ����
            float glitchIntensity = 0f;
            if (elapsed >= glitchDelayTime)
            {
                if (!glitchStarted)
                {
                    glitchStarted = true;
                    PlayGlitchSound();
                }

                // �۸�ġ ���� ������ ����
                glitchIntensity = Mathf.Lerp(0f, glitchIntensityMax, (elapsed - glitchDelayTime) / (dissolveDuration - glitchDelayTime));

                // ������ ���� �߿� �۸�ġ ���� �ƹ� ȿ��
                if (progress > 0.3f && progress < 0.8f)
                {
                    float pulse = (Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f);
                    glitchIntensity *= pulse;
                }
            }

            // ��� ������ ���� ���ÿ� ������Ʈ
            foreach (Material mat in dissolveMaterials)
            {
                mat.SetFloat("_DissolveAmount", dissolveAmount);
                mat.SetFloat("_StripeWidth", stripeWidth);
                mat.SetFloat("_StripeSpeed", stripeSpeed);
                mat.SetFloat("_GlitchIntensity", glitchIntensity);
                mat.SetFloat("_StripeIntensity", 0.5f + progress * 0.5f);
            }

            yield return null;
        }

        // ȿ�� �Ϸ� �� ������Ʈ ����
        Destroy(gameObject);
    }

    private void PlayDissolveSound()
    {
        if (audioSource != null && dissolveSound != null)
        {
            audioSource.clip = dissolveSound;
            audioSource.loop = true;
            audioSource.volume = 0.7f;
            audioSource.Play();
        }
    }

    private void PlayGlitchSound()
    {
        if (audioSource != null && glitchSound != null)
        {
            audioSource.PlayOneShot(glitchSound, 0.5f);
        }
    }

    private void OnDestroy()
    {
        // ��� ���� ����
        foreach (Material mat in dissolveMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
    }
}