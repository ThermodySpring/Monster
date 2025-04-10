using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Renderer))]
public class DataDissolutionEffect : MonoBehaviour
{
    // ���� ȿ�� ����
    [Header("���� ȿ�� ����")]
    [SerializeField] private float dissolutionDuration = 1.0f;  // ���� ���� �ð�
    [SerializeField] private float glitchIncreaseDuration = 0.4f; // �۸�ġ ���� �ð�
    [SerializeField] private float delayBeforeDissolve = 0.2f;  // ���� ���� �� ���� �ð�
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float yOffset = 1.0f;             // ���� �� Y�� �̵�
    [SerializeField] private Vector3 scatterForce = new Vector3(1f, 2f, 1f); // ���� �� ���� ����� ��
    [SerializeField] private Vector3 voxelRotationSpeed = new Vector3(1f, 1f, 1f); // ���� ȸ�� �ӵ�

    // �ð� ȿ�� ����
    [Header("�ð� ȿ�� ����")]
    [SerializeField] private Color glowColor = new Color(0f, 0.7f, 1f); // �߱� ����
    [SerializeField] private float emissionStrength = 3.0f;    // �߱� ����
    [SerializeField] private float finalVoxelSize = 0.05f;     // ���� ���� ũ��
    [SerializeField] private float voxelHollowAmount = 0.3f;   // ���� �߰�ȭ ����

    // ȿ�� ������Ʈ
    [Header("�߰� ȿ��")]
    [SerializeField] private GameObject glitchParticlePrefab;  // �۸�ġ ��ƼŬ ����Ʈ
    [SerializeField] private AudioClip dissolutionSound;        // ���� ����
    [SerializeField] private AudioClip glitchSound;             // �۸�ġ ����

    // ���� ����
    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private List<Material> originalMaterials = new List<Material>();
    private List<Material> dissolveMaterials = new List<Material>();
    private float dissolveAmount = 0f;
    private float glitchIntensity = 0f;
    private bool isDissolving = false;
    private Coroutine dissolutionCoroutine;
    private Rigidbody rigidBody;
    private Collider mainCollider;
    private NavMeshAgent navAgent;

    // �ݹ� �̺�Ʈ
    public System.Action OnDissolutionStarted;
    public System.Action OnDissolutionCompleted;

    private void Awake()
    {
        // �ʿ��� ������Ʈ ���� ��������
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        rigidBody = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
        navAgent = GetComponent<NavMeshAgent>();

        // ���� ��Ƽ���� ����
        originalMaterials.AddRange(rend.materials);

        // ���� ���̴� ��Ƽ���� ����
        CreateDissolveMaterials();
    }

    private void OnDestroy()
    {
        // �޸� ���� ������ ���� �ڿ� ����
        foreach (var mat in dissolveMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
    }

    // ���� ���̴��� ��Ƽ���� ����
    private void CreateDissolveMaterials()
    {
        Shader dissolveShader = Shader.Find("Custom/DataDissolution");
        if (dissolveShader == null)
        {
            Debug.LogError("DataDissolution ���̴��� ã�� �� �����ϴ�. ���̴��� ������Ʈ�� �߰��Ǿ����� Ȯ���ϼ���.");
            return;
        }

        // �� ���� ��Ƽ���󸶴� ���� ��Ƽ���� ����
        foreach (var originalMat in originalMaterials)
        {
            Material dissolveMat = new Material(dissolveShader);

            // ���� ��Ƽ���󿡼� �ؽ�ó�� ���� ����
            if (originalMat.HasProperty("_MainTex"))
            {
                dissolveMat.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
            }

            if (originalMat.HasProperty("_Color"))
            {
                dissolveMat.SetColor("_Color", originalMat.GetColor("_Color"));
            }

            // ������ �ؽ�ó ���� (������ �⺻ ������ �ؽ�ó ���)
            Texture2D noiseTex = Resources.Load<Texture2D>("Textures/NoiseTexture");
            if (noiseTex != null)
            {
                dissolveMat.SetTexture("_NoiseTex", noiseTex);
            }

            // �⺻ �Ӽ� ����
            dissolveMat.SetColor("_GlowColor", glowColor);
            dissolveMat.SetFloat("_DissolveAmount", 0f);
            dissolveMat.SetFloat("_DissolveScale", 1f);
            dissolveMat.SetFloat("_GlitchIntensity", 0f);
            dissolveMat.SetFloat("_VoxelSize", 0.01f);
            dissolveMat.SetFloat("_VoxelHollow", 0f);
            dissolveMat.SetFloat("_EmissionStrength", emissionStrength);

            dissolveMaterials.Add(dissolveMat);
        }
    }

    // ���� ȿ�� ����
    public void StartDissolve()
    {
        if (isDissolving) return;

        // ���� ���� ���� Coroutine�� ������ ����
        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
        }

        dissolutionCoroutine = StartCoroutine(DissolutionRoutine());
    }

    // ���� ȿ�� ���� (�� ���� ����)
    public void StopDissolve(bool resetToOriginal = true)
    {
        if (dissolutionCoroutine != null)
        {
            StopCoroutine(dissolutionCoroutine);
            dissolutionCoroutine = null;
        }

        isDissolving = false;

        // ���� ��Ƽ����� ����
        if (resetToOriginal)
        {
            rend.materials = originalMaterials.ToArray();
            dissolveAmount = 0f;
            glitchIntensity = 0f;
        }
    }

    // ���� ���� �ڷ�ƾ
    private IEnumerator DissolutionRoutine()
    {
        isDissolving = true;
        OnDissolutionStarted?.Invoke();

        // ���� ó�� ��Ȱ��ȭ
        DisablePhysics();

        // ���� ���̴� ��Ƽ����� ����
        rend.materials = dissolveMaterials.ToArray();

        // ���� �ð�
        if (delayBeforeDissolve > 0)
        {
            yield return new WaitForSeconds(delayBeforeDissolve);
        }

        // �۸�ġ ȿ�� ����
        float glitchTimer = 0f;
        while (glitchTimer < glitchIncreaseDuration)
        {
            glitchTimer += Time.deltaTime;
            glitchIntensity = Mathf.Lerp(0f, 0.5f, glitchTimer / glitchIncreaseDuration);

            // ���̴� �Ӽ� ������Ʈ
            UpdateShaderProperties();

            yield return null;
        }

        // �۸�ġ ���� ���
        PlayGlitchSound();

        // �۸�ġ ��ƼŬ ȿ�� ����
        SpawnGlitchParticles();

        // ���� ȿ�� ����
        float timer = 0f;
        while (timer < dissolutionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / dissolutionDuration;

            // �ִϸ��̼� Ŀ�� ����
            dissolveAmount = dissolveCurve.Evaluate(t);

            // �۸�ġ ȿ���� ���ذ� ����ʿ� ���� ����
            glitchIntensity = Mathf.Lerp(0.5f, 1.0f, t);

            // ���� ũ��� ���� Ŀ��
            float currentVoxelSize = Mathf.Lerp(0.01f, finalVoxelSize, t);

            // ���� �߰�ȭ ���� ����
            float currentHollow = Mathf.Lerp(0f, voxelHollowAmount, t);

            // ���̴� �Ӽ� ������Ʈ
            UpdateShaderProperties(currentVoxelSize, currentHollow);

            // ������Ʈ ȸ�� (���� ���� ȸ�� ȿ��)
            if (t > 0.5f)
            {
                transform.Rotate(voxelRotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // ���� �Ϸ� �� ������Ʈ ��Ȱ��ȭ �Ǵ� ����
        OnDissolutionCompleted?.Invoke();

        // ������ ���ص� ������Ʈ ó�� (������)
        // gameObject.SetActive(false);

        isDissolving = false;
    }

    // ���̴� �Ӽ� ������Ʈ
    private void UpdateShaderProperties(float? voxelSize = null, float? hollow = null)
    {
        for (int i = 0; i < rend.materials.Length; i++)
        {
            if (i < dissolveMaterials.Count)
            {
                Material mat = rend.materials[i];

                // �⺻ �Ӽ� ����
                mat.SetFloat("_DissolveAmount", dissolveAmount);
                mat.SetFloat("_GlitchIntensity", glitchIntensity);

                // ������ �Ӽ� ����
                if (voxelSize.HasValue)
                {
                    mat.SetFloat("_VoxelSize", voxelSize.Value);
                }

                if (hollow.HasValue)
                {
                    mat.SetFloat("_VoxelHollow", hollow.Value);
                }
            }
        }
    }

    // �۸�ġ ��ƼŬ ȿ�� ����
    private void SpawnGlitchParticles()
    {
        if (glitchParticlePrefab != null)
        {
            GameObject particles = Instantiate(glitchParticlePrefab, transform.position, Quaternion.identity);

            // ��ƼŬ �ý��� ���� (����, ũ�� ��)
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = glowColor;
            }

            // ���� �ð� �� ��ƼŬ ����
            Destroy(particles, 5f);
        }
    }

    // ���� ȿ�� ���
    private void PlayGlitchSound()
    {
        if (glitchSound != null)
        {
            AudioSource.PlayClipAtPoint(glitchSound, transform.position);
        }
    }

    private void PlayDissolutionSound()
    {
        if (dissolutionSound != null)
        {
            AudioSource.PlayClipAtPoint(dissolutionSound, transform.position);
        }
    }

    // ���� ó�� ��Ȱ��ȭ
    private void DisablePhysics()
    {
        if (rigidBody != null)
        {
            rigidBody.isKinematic = true;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        if (navAgent != null && navAgent.enabled)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
    }
}