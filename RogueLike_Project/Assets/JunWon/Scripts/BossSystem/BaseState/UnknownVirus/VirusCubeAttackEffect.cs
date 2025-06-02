using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ ���̷��� ť�� ������ ����Ʈ - �߽��� ���� ������ü ����
/// </summary>
public class VirusCubeAttackEffect : MonoBehaviour
{

    [Header("Enhanced Spread")]
    [SerializeField] private bool useEnhancedSpread = true;

    [SerializeField] private VirusCubeSpread spreadCalculator;

    [Header("Formation Settings")]
    [SerializeField] private float formationTime = 2f;        // ť�� ���� �ð�
    [SerializeField] private float compactTime = 1f;          // ���� �ð�
    [SerializeField] private float expandTime = 0.3f;         // ��ħ �ð�
    [SerializeField] private float returnTime = 1.5f;         // ���� ��ġ�� ���� �ð�
    [SerializeField] private bool shouldReturnToOriginal = true; // ���� ��ġ�� �������� ����

    [Header("Cube Formation")]
    [SerializeField] private Vector3Int cubeSize = new Vector3Int(8, 8, 8);
    [SerializeField] private float voxelSpacing = 0.15f;      // ���� ����
    [SerializeField] private float compactScale = 0.6f;       // ����� ������
    [SerializeField] private float expandScale = 1.8f;        // ��ħ�� ������
    [SerializeField] private Vector3 cubeCenter = Vector3.zero; // ť�� �߽���

    [Header("Visual Effects")]
    [SerializeField] private Color formationColor = Color.cyan;
    [SerializeField] private Color chargingColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private ParticleSystem expandEffect;
    [SerializeField] private Light coreLight;                 // �߽� ����Ʈ

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip formationSound;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip expandSound;

    // ���� ����
    private List<Transform> voxelChildren = new List<Transform>();
    private List<Vector3> originalPositions = new List<Vector3>();    // ���� ��ġ (���� �÷��� ��ġ)
    private List<Vector3> cubePositions = new List<Vector3>();        // ť�� ���� ��ġ
    private List<Vector3> compactPositions = new List<Vector3>();     // ���� ��ġ
    private List<Vector3> expandedPositions = new List<Vector3>();    // ��ħ ��ġ
    private VoxelFloatEffect floatEffect;

    // ������ ����
    private Transform target;
    private bool isExecuting = false;

    private void Start()
    {
        useEnhancedSpread = true;
        // ���� �ڽ� ��ü���� ������ ���
        CollectExistingVoxels();

        // ������Ʈ ����
        floatEffect = GetComponent<VoxelFloatEffect>();
        if (floatEffect == null)
            floatEffect = gameObject.AddComponent<VoxelFloatEffect>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // �÷��̾� Ÿ�� ã��
        target = GameObject.FindWithTag("Player")?.transform;

        // ť�� �߽����� ���� ��ġ�� ����
        cubeCenter = transform.position;

        if (useEnhancedSpread)
        {
            spreadCalculator = GetComponent<VirusCubeSpread>();
        }
    }

    /// <summary>
    /// ������ ���� ����
    /// </summary>
    public void StartLaserAttack()
    {
        if (isExecuting) return;
        StartCoroutine(ExecuteLaserAttack());
    }

    private IEnumerator ExecuteLaserAttack()
    {
        isExecuting = true;

        // 1. ť�� ���� (���� ��ġ���� �߽��� ���� ������ü��)
        yield return StartCoroutine(FormCubePhase());

        // 2. ���� �� ��¡
        yield return StartCoroutine(CompactAndChargePhase());

        // 3. ��ħ �� ������ �߻�
        yield return StartCoroutine(ExpandAndLaserPhase());

        // 4. ���� ��ġ�� ���� (���� �÷��� ����)
        if (shouldReturnToOriginal)
        {
            yield return StartCoroutine(ReturnToFloatingPhase());
        }
        isExecuting = false;
    }

    /// <summary>
    /// ���� �ڽ� ��ü���� ������ ����
    /// </summary>
    private void CollectExistingVoxels()
    {
        voxelChildren.Clear();
        originalPositions.Clear();

        // ��� �ڽ� ��ü ����
        foreach (Transform child in transform)
        {
            voxelChildren.Add(child);
            // ���� ��ġ�� ���� �÷��� ��ġ�� ����
            originalPositions.Add(child.localPosition);
        }

        // ������ü ��ġ�� ���
        CalculateFormationPositions();

        Debug.Log($"������ ���� ����: {voxelChildren.Count}");
    }

    /// <summary>
    /// �߽��� ���� ������ü ��ġ�� ���
    /// </summary>
    private void CalculateFormationPositions()
    {
        cubePositions.Clear();
        compactPositions.Clear();
        expandedPositions.Clear();

        int positionIndex = 0;
        int totalPositions = 0;

        // ���� �� ��ġ ���� ���
        for (int x = 0; x < cubeSize.x; x++)
        {
            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++)
                {
                    bool isEdge = (x == 0 || x == cubeSize.x - 1) ||
                                  (y == 0 || y == cubeSize.y - 1) ||
                                  (z == 0 || z == cubeSize.z - 1);
                    if (isEdge) totalPositions++;
                }
            }
        }

        // ���� ��ġ ���
        for (int x = 0; x < cubeSize.x; x++)
        {
            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++)
                {
                    bool isEdge = (x == 0 || x == cubeSize.x - 1) ||
                                  (y == 0 || y == cubeSize.y - 1) ||
                                  (z == 0 || z == cubeSize.z - 1);

                    if (isEdge)
                    {
                        // �߽��� �������� ������ü ��ġ ���
                        Vector3 cubePos = new Vector3(
                            (x - cubeSize.x / 2f + 0.5f) * voxelSpacing,
                            (y - cubeSize.y / 2f + 0.5f) * voxelSpacing,
                            (z - cubeSize.z / 2f + 0.5f) * voxelSpacing
                        );

                        cubePositions.Add(cubePos);

                        // ���� ��ġ
                        Vector3 compactPos = cubePos * compactScale;
                        compactPositions.Add(compactPos);

                        // ���� ���� ��ġ ���
                        Vector3 expandPos;
                        if (useEnhancedSpread && spreadCalculator != null)
                        {
                            Debug.Log("���� ���� ���");
                            expandPos = spreadCalculator.CalculateSpreadPosition(cubePos, positionIndex, totalPositions);
                        }
                        else
                        {
                            // �⺻ ���� (�յ��ϰ�)
                            Vector3 direction = cubePos.normalized;
                            expandPos = direction * expandScale;
                        }

                        expandedPositions.Add(expandPos);
                        positionIndex++;
                    }
                }
            }
        }

        Debug.Log($"���� ť�� ��ġ ����: {cubePositions.Count}");
    }

    /// <summary>
    /// 1�ܰ�: ������ü ����
    /// </summary>
    private IEnumerator FormCubePhase()
    {
        PlaySound(formationSound);
        Debug.Log("������ü ���� ����");

        float elapsed = 0f;
        while (elapsed < formationTime)
        {
            float progress = elapsed / formationTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // �� ������ ������ü ��ġ�� �̵�
            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 startPos = originalPositions[i];              // ���� ���� ��ġ
                Vector3 targetPos = i < cubePositions.Count ? cubePositions[i] : Vector3.zero; // ������ü ��ġ

                voxelChildren[i].localPosition = Vector3.Lerp(startPos, targetPos, easedProgress);

                // �����Ǹ鼭 ȸ��
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 180f * (1f - easedProgress));

                // ���� ��ȭ
                Color lerpColor = Color.Lerp(formationColor, chargingColor, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("������ü ���� �Ϸ�");
    }

    /// <summary>
    /// 2�ܰ�: ���� �� ��¡
    /// </summary>
    private IEnumerator CompactAndChargePhase()
    {
        PlaySound(chargeSound);
        Debug.Log("���� �� ��¡ ����");

        // ��¡ ��ƼŬ ����
        if (chargeEffect != null)
        {
            chargeEffect.gameObject.SetActive(true);
            chargeEffect.Play();
        }

        // �ھ� ����Ʈ Ȱ��ȭ
        if (coreLight != null)
        {
            coreLight.enabled = true;
            coreLight.color = chargingColor;
            coreLight.transform.localPosition = Vector3.zero; // �߽ɿ� ��ġ
        }

        float elapsed = 0f;
        while (elapsed < compactTime)
        {
            float progress = elapsed / compactTime;
            float intensity = Mathf.PingPong(elapsed * 4f, 1f);

            // ������ü�� �߽����� ����
            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 cubePos = i < cubePositions.Count ? cubePositions[i] : Vector3.zero;
                Vector3 compactPos = i < compactPositions.Count ? compactPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(cubePos, compactPos, progress);

                // ��¡ ���� �޽�
                Color pulseColor = Color.Lerp(chargingColor, attackColor, intensity);
            }

            // ����Ʈ ���� ����
            if (coreLight != null)
            {
                coreLight.intensity = 2f + intensity * 3f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("���� �� ��¡ �Ϸ�");
    }

    /// <summary>
    /// 3�ܰ�: ��ħ �� ������ �߻�
    /// </summary>
    private IEnumerator ExpandAndLaserPhase()
    {
        PlaySound(expandSound);
        Debug.Log("��ħ �� ������ �߻� ����");

        // ��¡ ȿ�� ����
        if (chargeEffect != null)
            chargeEffect.Stop();

        // ��ħ ȿ�� ����
        if (expandEffect != null)
        {
            expandEffect.gameObject.SetActive(true);
            expandEffect.Play();
        }

        // ���� ��ħ �ִϸ��̼�
        float elapsed = 0f;
        while (elapsed < expandTime)
        {
            float progress = elapsed / expandTime;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // ���� �

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                Vector3 compactPos = i < compactPositions.Count ? compactPositions[i] : Vector3.zero;
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(compactPos, expandPos, easedProgress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 4�ܰ�: ���� ���� �÷��� ���·� ����
    /// </summary>
    private IEnumerator ReturnToFloatingPhase()
    {
        Debug.Log("���� �÷��� ���·� ���� ����");

        float elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                // ���� ��ħ ��ġ���� ���� ���� �÷��� ��ġ�� ����
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : voxelChildren[i].localPosition;
                Vector3 originalPos = i < originalPositions.Count ? originalPositions[i] : Vector3.zero;

                voxelChildren[i].localPosition = Vector3.Lerp(expandPos, originalPos, easedProgress);

                // ������ ����
                float scale = Mathf.Lerp(voxelChildren[i].localScale.x, 1f, easedProgress);
                voxelChildren[i].localScale = Vector3.one * scale;

                // �����ϸ鼭 �ε巯�� ȸ��
                voxelChildren[i].Rotate(Vector3.up, Time.deltaTime * 90f * (1f - progress));
            }

            // ����Ʈ ������ ����
            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(5f, 0f, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ������ ���� ���·� ����
        RestoreOriginalFloatingState();

        Debug.Log("���� �÷��� ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ���� �÷��� ���·� ���� ����
    /// </summary>
    private void RestoreOriginalFloatingState()
    {
        for (int i = 0; i < voxelChildren.Count && i < originalPositions.Count; i++)
        {
            // ��ġ ���� ����
            voxelChildren[i].localPosition = originalPositions[i];

            // ������ ����
            voxelChildren[i].localScale = Vector3.one;

        }

        // ����Ʈ ����
        if (coreLight != null)
            coreLight.enabled = false;

        if (chargeEffect != null)
            chargeEffect.gameObject.SetActive(false);

        if (expandEffect != null)
            expandEffect.gameObject.SetActive(false);

        // �÷��� ȿ�� ��Ȱ��ȭ
        if (floatEffect != null)
        {
            floatEffect.enabled = true;
            floatEffect.SetFloatIntensity(1f);
        }
    }


    /// <summary>
    /// 5�ܰ�: �Ҹ� (�������� ���� ��)
    /// </summary>
    private IEnumerator DissolvePhase()
    {
        Debug.Log("���̷��� ť�� �Ҹ� ����");

        float elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;

            for (int i = 0; i < voxelChildren.Count; i++)
            {
                // ������ ����
                float scale = Mathf.Lerp(1f, 0f, progress);
                voxelChildren[i].localScale = Vector3.one * scale;

                // ���� ȸ��
                voxelChildren[i].Rotate(Random.insideUnitSphere, Time.deltaTime * 360f);

                // �����ϰ� �����
                Vector3 randomOffset = Random.insideUnitSphere * progress * 3f;
                Vector3 expandPos = i < expandedPositions.Count ? expandedPositions[i] : voxelChildren[i].localPosition;
                voxelChildren[i].localPosition = expandPos + randomOffset;
            }

            // ����Ʈ ���̵�ƿ�
            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(5f, 0f, progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ����
        if (coreLight != null)
            coreLight.enabled = false;

        // ��ü ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopEffect()
    {
        StopAllCoroutines();
        isExecuting = false;

        if (shouldReturnToOriginal)
        {
            RestoreOriginalFloatingState();
        }
    }

    public void SetReturnMode(bool shouldReturn)
    {
        shouldReturnToOriginal = shouldReturn;
    }

    /// <summary>
    /// ������ ����� - ������ü ���� ��ġ ǥ��
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // ť�� �߽��� ǥ��
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // ������ü ���� ǥ��
        Gizmos.color = Color.cyan;
        Vector3 cubeExtents = Vector3.one * cubeSize.x * voxelSpacing;
        Gizmos.DrawWireCube(transform.position, cubeExtents);

        // ���� ���� ǥ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, cubeExtents * compactScale);

        // ��ħ ���� ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cubeExtents * expandScale);
    }
}