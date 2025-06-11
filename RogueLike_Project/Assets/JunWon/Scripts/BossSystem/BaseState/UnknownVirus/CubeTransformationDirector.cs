using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 292���� ���̷��� ť�� ���� �ý��� - Base ���� ��ü ���
/// </summary>
public class CubeTransformationDirector : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform baseContainer; // Base ��ü ����
    [SerializeField] private bool autoFindBaseContainer = true;

    [Header("Transformation Patterns")]
    [SerializeField] private TransformPattern currentPattern = TransformPattern.Implosion;
    [SerializeField] private float transformDuration = 3f;
    [SerializeField] private Ease transformEase = Ease.OutCubic;

    [Header("Cube Formation Settings")]
    [SerializeField] private Vector3 cubeCenter = Vector3.zero;
    [SerializeField] private float cubeSize = 6f;
    [SerializeField] private Vector3Int cubeGridSize = new Vector3Int(8, 8, 8); // 8x8x8 = 512, ������ 292���� ���
    [SerializeField] private float voxelSpacing = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem assemblyEffect;
    [SerializeField] private ParticleSystem energyAura;
    [SerializeField] private ParticleSystem completionBurst;
    [SerializeField] private Light coreLight;
    [SerializeField] private AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 5);
    [SerializeField] private Color[] transformColors = { Color.cyan, Color.yellow, Color.red };

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] transformSounds;
    [SerializeField] private AudioClip completionSound;

    [Header("Performance")]
    [SerializeField] private int batchSize = 10; // ���ÿ� �̵��� ���� ��
    [SerializeField] private float batchDelay = 0.05f; // ��ġ �� ������

    // �ٽ� ������Ʈ��
    private VoxelFloatEffect floatEffect;
    private VirusCubeStateManager stateManager;

    // 292�� ���� ����
    [SerializeField] private List<Transform> voxels = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> formationPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, int> voxelIndices = new Dictionary<Transform, int>();

    // ���� ���� ����
    private bool isTransforming = false;
    private bool isInCubeForm = false;

    // ť�� ���� ��ǥ ����
    private List<Vector3> availableCubePositions = new List<Vector3>();

    public enum TransformPattern
    {
        Spiral,      // ���������� ����
        Wave,        // �ĵ�ó�� ������ ����  
        Implosion,   // �߽����� �޼� ����
        Organic,     // ����üó�� �ڿ������� ����
        Glitch,      // �۸�ġ ȿ���� �Բ� ����
        Magnetic,    // �ڱ���ó�� �������� ȿ��
        Sequential,  // ������ ����
        Explosion    // ���� �� ������
    }

    #region Initialization

    void Start()
    {
        InitializeComponents();
        FindOrAssignBaseContainer();
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();

        Debug.Log($"[CubeTransformation] �ʱ�ȭ �Ϸ� - {voxels.Count}�� ���� �߰�");
    }

    private void InitializeComponents()
    {
        floatEffect = GetComponent<VoxelFloatEffect>();
        stateManager = GetComponent<VirusCubeStateManager>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // DOTween �ʱ�ȭ
        DOTween.Init();
    }

    private void FindOrAssignBaseContainer()
    {
        if (baseContainer == null && autoFindBaseContainer)
        {
            // Base��� �̸��� ���� ��ü ã��
            baseContainer = transform.Find("Base");

            if (baseContainer == null)
            {
                Debug.LogError("[CubeTransformation] Base �����̳ʸ� ã�� �� �����ϴ�!");
                return;
            }
        }

        Debug.Log($"[CubeTransformation] Base �����̳� ����: {baseContainer.name}");
    }

    private void CollectVoxelsFromBase()
    {
        if (baseContainer == null)
        {
            Debug.LogError("[CubeTransformation] Base �����̳ʰ� �������� �ʾҽ��ϴ�!");
            return;
        }

        voxels.Clear();
        originalPositions.Clear();
        voxelIndices.Clear();

        // Base ������ ��� �ڽ� ��ü�� ������ ����
        int index = 0;
        foreach (Transform child in baseContainer)
        {
            voxels.Add(child);
            originalPositions[child] = child.localPosition;
            voxelIndices[child] = index;
            index++;
        }

        Debug.Log($"[CubeTransformation] {voxels.Count}�� ������ Base���� �����߽��ϴ�.");
    }

    private void CalculateAllCubePositions()
    {
        availableCubePositions.Clear();

        // 8x8x8 ������ü�� �ܰ� �鸸 ����Ͽ� ��ġ ���
        for (int x = 0; x < cubeGridSize.x; x++)
        {
            for (int y = 0; y < cubeGridSize.y; y++)
            {
                for (int z = 0; z < cubeGridSize.z; z++)
                {
                    // ������ü�� �ܰ����� Ȯ�� (6�� �� �ϳ� �̻� ����)
                    bool isOnFace = (x == 0 || x == cubeGridSize.x - 1) ||
                                    (y == 0 || y == cubeGridSize.y - 1) ||
                                    (z == 0 || z == cubeGridSize.z - 1);

                    if (isOnFace)
                    {
                        Vector3 position = new Vector3(
                            (x - cubeGridSize.x / 2f + 0.5f) * voxelSpacing,
                            (y - cubeGridSize.y / 2f + 0.5f) * voxelSpacing,
                            (z - cubeGridSize.z / 2f + 0.5f) * voxelSpacing
                        ) + cubeCenter;

                        availableCubePositions.Add(position);
                    }
                }
            }
        }

        Debug.Log($"[CubeTransformation] {availableCubePositions.Count}���� ť�� ��ġ ��� �Ϸ�");
    }

    private void AssignFormationPositions()
    {
        formationPositions.Clear();

        // ��� ������ ��ġ�� ���� ������ ������ ���
        if (availableCubePositions.Count < voxels.Count)
        {
            Debug.LogWarning($"[CubeTransformation] ���� ��({voxels.Count})�� ��� ������ ��ġ ��({availableCubePositions.Count})���� �����ϴ�!");
        }

        // �� ������ ���� ���� ����� ť�� ��ġ �Ҵ�
        var assignedPositions = new HashSet<Vector3>();

        for (int i = 0; i < voxels.Count && i < availableCubePositions.Count; i++)
        {
            Transform voxel = voxels[i];
            Vector3 bestPosition = Vector3.zero;
            float minDistance = float.MaxValue;

            // ���� �Ҵ���� ���� ��ġ �߿��� ���� ����� �� ã��
            foreach (var pos in availableCubePositions)
            {
                if (assignedPositions.Contains(pos)) continue;

                float distance = Vector3.Distance(voxel.localPosition, pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestPosition = pos;
                }
            }

            if (bestPosition != Vector3.zero)
            {
                formationPositions[voxel] = bestPosition;
                assignedPositions.Add(bestPosition);
            }
        }

        Debug.Log($"[CubeTransformation] {formationPositions.Count}�� ������ ���� ���� ��ġ �Ҵ� �Ϸ�");
    }

    #endregion

    #region Public API

    /// <summary>
    /// ť�� ���·� ���� ����
    /// </summary>
    public void StartCubeTransformation()
    {
        if (isTransforming)
        {
            Debug.LogWarning("[CubeTransformation] �̹� ���� ���Դϴ�!");
            return;
        }

        Debug.Log($"[CubeTransformation] ť�� ���� ���� - ����: {currentPattern}");
        StartCoroutine(ExecuteTransformation());
    }

    /// <summary>
    /// ���� ���·� �ǵ�����
    /// </summary>
    public void RevertToOriginal()
    {
        if (isTransforming)
        {
            Debug.LogWarning("[CubeTransformation] ���� �߿��� �ǵ��� �� �����ϴ�!");
            return;
        }

        Debug.Log("[CubeTransformation] ���� ���� ����");
        StartCoroutine(ExecuteReversion());
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public void SetTransformPattern(TransformPattern pattern)
    {
        currentPattern = pattern;
        Debug.Log($"[CubeTransformation] ���� ���� ����: {pattern}");
    }

    /// <summary>
    /// ���� ���� �ð� ����
    /// </summary>
    public void SetTransformDuration(float duration)
    {
        transformDuration = Mathf.Max(0.5f, duration);
    }

    /// <summary>
    /// ���� ���� ���� Ȯ��
    /// </summary>
    public bool IsInCubeForm => isInCubeForm;
    public bool IsTransforming => isTransforming;

    #endregion

    #region Transformation Execution

    private IEnumerator ExecuteTransformation()
    {
        isTransforming = true;

        // 1. �غ� �ܰ�
        PrepareTransformation();
        yield return new WaitForSeconds(0.5f);

        // 2. ���Ϻ� ��ȯ ����
        yield return StartCoroutine(ExecutePatternTransformation());

        // 3. �ϼ� �ܰ�
        yield return StartCoroutine(FinalizeFormation());

        // 4. �ϼ� ȿ��
        PlayCompletionEffects();

        isInCubeForm = true;
        isTransforming = false;

        Debug.Log("[CubeTransformation] ť�� ���� �Ϸ�!");
    }

    private void PrepareTransformation()
    {
        // ���� ȿ���� ���� ���� ��ȯ
        if (floatEffect != null)
        {
            floatEffect.SetChargingMode(true);
        }

        // �ھ� ����Ʈ Ȱ��ȭ
        if (coreLight != null)
        {
            coreLight.enabled = true;
            coreLight.color = transformColors[0]; // ���� ����
            coreLight.transform.position = transform.position + cubeCenter;

            DOTween.To(() => coreLight.intensity, x => coreLight.intensity = x, 3f, 0.5f);
        }

        // ���� ��ƼŬ ����
        if (assemblyEffect != null)
        {
            assemblyEffect.transform.position = transform.position + cubeCenter;
            assemblyEffect.Play();
        }

        PlayRandomTransformSound();
    }

    private IEnumerator ExecutePatternTransformation()
    {
        switch (currentPattern)
        {
            case TransformPattern.Spiral:
                yield return StartCoroutine(SpiralTransformation());
                break;
            case TransformPattern.Wave:
                yield return StartCoroutine(WaveTransformation());
                break;
            case TransformPattern.Implosion:
                yield return StartCoroutine(ImplosionTransformation());
                break;
            case TransformPattern.Organic:
                yield return StartCoroutine(OrganicTransformation());
                break;
            case TransformPattern.Glitch:
                yield return StartCoroutine(GlitchTransformation());
                break;
            case TransformPattern.Magnetic:
                yield return StartCoroutine(MagneticTransformation());
                break;
            case TransformPattern.Sequential:
                yield return StartCoroutine(SequentialTransformation());
                break;
            case TransformPattern.Explosion:
                yield return StartCoroutine(ExplosionTransformation());
                break;
        }
    }

    #endregion

    #region Transformation Patterns

    private IEnumerator SpiralTransformation()
    {
        var sortedVoxels = SortVoxelsByDistanceFromCenter();
        int batchCount = 0;

        for (int i = 0; i < sortedVoxels.Count; i++)
        {
            var voxel = sortedVoxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            float delay = (i / (float)batchSize) * batchDelay;
            Vector3 spiralWaypoint = CalculateSpiralPath(i, sortedVoxels.Count);
            Vector3 targetPos = formationPositions[voxel];

            StartCoroutine(MoveVoxelAlongPath(voxel, spiralWaypoint, targetPos, delay));

            batchCount++;
            if (batchCount >= batchSize)
            {
                yield return new WaitForSeconds(batchDelay);
                batchCount = 0;
            }
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator WaveTransformation()
    {
        var layeredVoxels = GroupVoxelsByLayer();

        for (int layerIndex = 0; layerIndex < layeredVoxels.Count; layerIndex++)
        {
            var layer = layeredVoxels[layerIndex];

            foreach (var voxel in layer)
            {
                if (!formationPositions.ContainsKey(voxel)) continue;

                Vector3 targetPos = formationPositions[voxel];
                Vector3 waveHeight = targetPos + Vector3.up * (3f + layerIndex * 0.5f);

                // �ĵ� ȿ���� �̵�
                var sequence = DOTween.Sequence();
                sequence.Append(voxel.DOLocalMove(waveHeight, transformDuration * 0.4f).SetEase(Ease.OutQuad));
                sequence.Append(voxel.DOLocalMove(targetPos, transformDuration * 0.6f).SetEase(Ease.InOutBounce));

                voxel.DOLocalRotate(Vector3.zero, transformDuration);
            }

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator ImplosionTransformation()
    {
        List<Tween> allTweens = new List<Tween>();

        foreach (var voxel in voxels)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            // �޼��� ���� ȿ��
            var moveTween = voxel.DOLocalMove(targetPos, transformDuration * 0.7f)
                                 .SetEase(Ease.InExpo);

            var scaleTween = voxel.DOScale(Vector3.one * 0.2f, transformDuration * 0.2f)
                                  .SetEase(Ease.InQuad)
                                  .OnComplete(() => {
                                      voxel.DOScale(Vector3.one, transformDuration * 0.5f)
                                           .SetEase(Ease.OutBounce);
                                  });

            allTweens.Add(moveTween);
            allTweens.Add(scaleTween);
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator OrganicTransformation()
    {
        var clusters = CreateVoxelClusters();

        foreach (var cluster in clusters)
        {
            yield return StartCoroutine(GrowCluster(cluster));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }

    private IEnumerator GlitchTransformation()
    {
        if (floatEffect != null)
        {
            floatEffect.SetGlitchMode(true, 3f);
        }

        // ������ ������ ������ �۸�ġ �ڷ���Ʈ
        var shuffledVoxels = new List<Transform>(voxels);
        for (int i = 0; i < shuffledVoxels.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledVoxels.Count);
            var temp = shuffledVoxels[i];
            shuffledVoxels[i] = shuffledVoxels[randomIndex];
            shuffledVoxels[randomIndex] = temp;
        }

        foreach (var voxel in shuffledVoxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                StartCoroutine(GlitchTeleport(voxel));
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            }
        }

        yield return new WaitForSeconds(transformDuration);

        if (floatEffect != null)
        {
            floatEffect.SetGlitchMode(false);
        }
    }

    private IEnumerator MagneticTransformation()
    {
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                StartCoroutine(MagneticPull(voxel));
                yield return new WaitForSeconds(0.02f);
            }
        }

        yield return new WaitForSeconds(transformDuration);
    }

    private IEnumerator SequentialTransformation()
    {
        // �ε��� ������� �������� �̵�
        for (int i = 0; i < voxels.Count; i++)
        {
            var voxel = voxels[i];
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            voxel.DOLocalMove(targetPos, 0.5f).SetEase(Ease.OutBack);
            voxel.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutQuad)
                 .OnComplete(() => voxel.DOScale(Vector3.one, 0.2f));

            // ���� �������� ��ġ ó��
            if (i % batchSize == 0)
                yield return new WaitForSeconds(batchDelay);
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ExplosionTransformation()
    {
        // 1�ܰ�: �����Ͽ� �����
        foreach (var voxel in voxels)
        {
            Vector3 explosionDirection = Random.insideUnitSphere.normalized;
            Vector3 explosionPos = voxel.localPosition + explosionDirection * Random.Range(5f, 10f);

            voxel.DOLocalMove(explosionPos, transformDuration * 0.3f).SetEase(Ease.OutExpo);
            voxel.DORotate(Random.insideUnitSphere * 360f, transformDuration * 0.3f);
        }

        yield return new WaitForSeconds(transformDuration * 0.4f);

        // 2�ܰ�: ť�� ���·� ������
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                Vector3 targetPos = formationPositions[voxel];
                voxel.DOLocalMove(targetPos, transformDuration * 0.6f).SetEase(Ease.InOutCubic);
                voxel.DOLocalRotate(Vector3.zero, transformDuration * 0.6f);
            }
        }

        yield return new WaitForSeconds(transformDuration * 0.6f);
    }

    #endregion

    #region Helper Methods

    private Vector3 CalculateSpiralPath(int index, int totalCount)
    {
        float progress = index / (float)totalCount;
        float angle = progress * Mathf.PI * 6f; // 3���� ����
        float radius = 4f * (1f - progress);
        float height = Mathf.Sin(progress * Mathf.PI) * 3f;

        return cubeCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );
    }

    private List<Transform> SortVoxelsByDistanceFromCenter()
    {
        var sorted = new List<Transform>(voxels);
        Vector3 centerWorld = transform.TransformPoint(cubeCenter);

        sorted.Sort((a, b) => {
            float distA = Vector3.Distance(a.position, centerWorld);
            float distB = Vector3.Distance(b.position, centerWorld);
            return distA.CompareTo(distB);
        });

        return sorted;
    }

    private List<List<Transform>> GroupVoxelsByLayer()
    {
        var layers = new List<List<Transform>>();
        var sorted = new List<Transform>(voxels);

        // Y ��ǥ �������� ����
        sorted.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        // ������ �׷�ȭ (�� ������ ���̾)
        float layerThickness = 1f;
        var currentLayer = new List<Transform>();
        float currentY = sorted[0].localPosition.y;

        foreach (var voxel in sorted)
        {
            if (Mathf.Abs(voxel.localPosition.y - currentY) > layerThickness)
            {
                if (currentLayer.Count > 0)
                {
                    layers.Add(currentLayer);
                    currentLayer = new List<Transform>();
                }
                currentY = voxel.localPosition.y;
            }
            currentLayer.Add(voxel);
        }

        if (currentLayer.Count > 0)
            layers.Add(currentLayer);

        return layers;
    }

    private IEnumerator MoveVoxelAlongPath(Transform voxel, Vector3 waypoint, Vector3 target, float delay)
    {
        yield return new WaitForSeconds(delay);

        // ��������Ʈ�� ���� ��ǥ������ �̵�
        var sequence = DOTween.Sequence();
        sequence.Append(voxel.DOLocalMove(waypoint, transformDuration * 0.4f).SetEase(Ease.OutQuad));
        sequence.Append(voxel.DOLocalMove(target, transformDuration * 0.6f).SetEase(Ease.InOutCubic));

        // ȸ���� �ε巴��
        sequence.Join(voxel.DOLocalRotate(Vector3.zero, transformDuration));
    }

    private IEnumerator GlitchTeleport(Transform voxel)
    {
        if (!formationPositions.ContainsKey(voxel)) yield break;

        Vector3 targetPos = formationPositions[voxel];
        Vector3 glitchOffset = Random.insideUnitSphere * 1f;

        // �۸�ġ ȿ��: ��� ������ٰ� ��ǥ ��ġ ��ó�� ��Ÿ��
        voxel.DOScale(Vector3.zero, 0.1f).OnComplete(() => {
            voxel.localPosition = targetPos + glitchOffset;
            voxel.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
            voxel.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutElastic);
        });

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator MagneticPull(Transform voxel)
    {
        if (!formationPositions.ContainsKey(voxel)) yield break;

        Vector3 targetPos = formationPositions[voxel];
        Vector3 currentPos = voxel.localPosition;

        // �ڱ��� ȿ��: � ��η� ������
        Vector3 midPoint1 = Vector3.Lerp(currentPos, targetPos, 0.33f) + Random.insideUnitSphere * 2f;
        Vector3 midPoint2 = Vector3.Lerp(currentPos, targetPos, 0.66f) + Random.insideUnitSphere * 1f;

        Vector3[] path = new Vector3[] { currentPos, midPoint1, midPoint2, targetPos };

        voxel.DOLocalPath(path, transformDuration * 0.8f, PathType.CatmullRom)
             .SetEase(Ease.InOutQuad);

        yield return null;
    }

    private List<List<Transform>> CreateVoxelClusters()
    {
        var clusters = new List<List<Transform>>();
        var remaining = new List<Transform>(voxels);

        while (remaining.Count > 0)
        {
            var cluster = new List<Transform>();
            var seed = remaining[Random.Range(0, remaining.Count)];
            cluster.Add(seed);
            remaining.Remove(seed);

            // ��ó�� �������� Ŭ�����Ϳ� �߰� (�� ���� Ŭ������ ũ��)
            int clusterSize = Random.Range(5, 15);
            for (int i = 0; i < clusterSize && remaining.Count > 0; i++)
            {
                Transform closest = null;
                float minDist = float.MaxValue;

                foreach (var voxel in remaining)
                {
                    float dist = Vector3.Distance(seed.localPosition, voxel.localPosition);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = voxel;
                    }
                }

                if (closest != null)
                {
                    cluster.Add(closest);
                    remaining.Remove(closest);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private IEnumerator GrowCluster(List<Transform> cluster)
    {
        foreach (var voxel in cluster)
        {
            if (!formationPositions.ContainsKey(voxel)) continue;

            Vector3 targetPos = formationPositions[voxel];

            // ������ ���� ȿ��
            voxel.DOScale(Vector3.zero, 0.1f).OnComplete(() => {
                voxel.localPosition = targetPos;
                voxel.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutElastic);
            });

            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
        }
    }

    #endregion

    #region Finalization & Effects

    private IEnumerator FinalizeFormation()
    {
        // ��� ������ ��Ȯ�� ��ġ�� �̼� ����
        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                voxel.DOLocalMove(formationPositions[voxel], 0.3f).SetEase(Ease.OutQuad);
                voxel.DOLocalRotate(Vector3.zero, 0.3f);
                voxel.DOScale(Vector3.one, 0.2f);
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void PlayCompletionEffects()
    {
        // �ϼ� ��ƼŬ ȿ��
        if (completionBurst != null)
        {
            completionBurst.transform.position = transform.position + cubeCenter;
            completionBurst.Play();
        }

        if (energyAura != null)
        {
            energyAura.transform.position = transform.position + cubeCenter;
            energyAura.Play();
        }

        // ����Ʈ ȿ��
        if (coreLight != null)
        {
            coreLight.color = transformColors[2]; // �ϼ� ���� (������)
            coreLight.DOIntensity(8f, 0.2f).OnComplete(() => {
                coreLight.DOIntensity(3f, 0.8f);
            });
        }

        // �ϼ� ����
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }

        // ���� ȿ���� ���� ���� ��ȯ
        if (floatEffect != null)
        {
            floatEffect.SetAttackMode(true);
        }

        Debug.Log("[CubeTransformation] ť�� ���� �Ϸ�!");
    }

    #endregion

    #region Reversion

    private IEnumerator ExecuteReversion()
    {
        isTransforming = true;

        Debug.Log("[CubeTransformation] ���� ���� ����");

        // ���� �غ�
        PrepareReversion();
        yield return new WaitForSeconds(0.3f);

        // ���� ���� ����
        yield return StartCoroutine(RevertToOriginalPositions());

        // ���� �Ϸ� ó��
        CompleteReversion();

        isInCubeForm = false;
        isTransforming = false;

        Debug.Log("[CubeTransformation] ���� ���� �Ϸ�!");
    }

    private void PrepareReversion()
    {
        // ����Ʈ ȿ�� ����
        if (coreLight != null)
        {
            coreLight.color = transformColors[0]; // ���� ��������
            coreLight.DOIntensity(1f, 0.5f);
        }

        // ���� ����
        PlayRandomTransformSound();
    }

    private IEnumerator RevertToOriginalPositions()
    {
        if (stateManager != null)
        {
            // StateManager�� ����� �ε巯�� ����
            yield return StartCoroutine(stateManager.RestoreOriginalStateSmooth(transformDuration));
        }
        else
        {
            // ���� ���� ó��
            foreach (var voxel in voxels)
            {
                if (originalPositions.ContainsKey(voxel))
                {
                    voxel.DOLocalMove(originalPositions[voxel], transformDuration)
                         .SetEase(Ease.OutCubic);

                    voxel.DOLocalRotate(Vector3.zero, transformDuration * 0.8f);

                    // �ణ�� ������ �־� �ڿ������� ��ü ȿ��
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
                }
            }

            yield return new WaitForSeconds(transformDuration);
        }
    }

    private void CompleteReversion()
    {
        // ����Ʈ ����
        if (coreLight != null)
        {
            coreLight.DOIntensity(0f, 0.5f).OnComplete(() => {
                coreLight.enabled = false;
            });
        }

        // ��ƼŬ ����
        if (assemblyEffect != null && assemblyEffect.isPlaying)
        {
            assemblyEffect.Stop();
        }

        if (energyAura != null && energyAura.isPlaying)
        {
            energyAura.Stop();
        }

        // ���� ȿ�� ����
        if (floatEffect != null)
        {
            floatEffect.SetFloatIntensity(1f);
            floatEffect.SetAttackMode(false);
        }
    }

    #endregion

    #region Audio

    private void PlayRandomTransformSound()
    {
        if (audioSource != null && transformSounds.Length > 0)
        {
            var sound = transformSounds[Random.Range(0, transformSounds.Length)];
            audioSource.PlayOneShot(sound);
        }
    }

    #endregion

    #region Debug & Testing

    [Header("Debug & Testing")]
    [SerializeField] private bool enableTestKeys = true;
    [SerializeField] private bool showDebugGizmos = true;

    void Update()
    {
        if (enableTestKeys && Application.isPlaying)
        {
            HandleTestInput();
        }

        // �������� ����Ʈ ���� ���� (ť�� ������ ��)
        if (isInCubeForm && coreLight != null && lightIntensityCurve != null)
        {
            float normalizedTime = (Time.time % 4f) / 4f; // 4�� �ֱ�
            float intensity = lightIntensityCurve.Evaluate(normalizedTime) * 3f + 2f;
            coreLight.intensity = intensity;
        }
    }

    private void HandleTestInput()
    {
        // TŰ�� ���� �׽�Ʈ
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!isTransforming)
            {
                if (isInCubeForm)
                {
                    Debug.Log("=== TŰ ����: ���� ���� ���� ===");
                    RevertToOriginal();
                }
                else
                {
                    Debug.Log("=== TŰ ����: ť�� ���� ���� ===");
                    StartCubeTransformation();
                }
            }
        }

        // RŰ�� ���� ����
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isTransforming && isInCubeForm)
            {
                Debug.Log("=== RŰ ����: ���� ���� ���� ===");
                RevertToOriginal();
            }
        }

        // ���� Ű�� ���� ����
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetTransformPattern(TransformPattern.Spiral); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetTransformPattern(TransformPattern.Wave); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetTransformPattern(TransformPattern.Implosion); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetTransformPattern(TransformPattern.Organic); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetTransformPattern(TransformPattern.Glitch); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { SetTransformPattern(TransformPattern.Magnetic); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { SetTransformPattern(TransformPattern.Sequential); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { SetTransformPattern(TransformPattern.Explosion); }

        // +/- Ű�� ���� �ӵ� ����
        if (Input.GetKeyDown(KeyCode.Equals)) // + Ű
        {
            SetTransformDuration(transformDuration - 0.5f);
            Debug.Log($"���� �ӵ� ����: {transformDuration}��");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetTransformDuration(transformDuration + 0.5f);
            Debug.Log($"���� �ӵ� ����: {transformDuration}��");
        }

        // CŰ�� ���� �� Ȯ��
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"=== ���� ���� ===");
            Debug.Log($"��ü ���� ��: {voxels.Count}");
            Debug.Log($"���� ��ġ �Ҵ� ��: {formationPositions.Count}");
            Debug.Log($"��� ������ ť�� ��ġ ��: {availableCubePositions.Count}");
            Debug.Log($"���� ����: {(isInCubeForm ? "ť��" : "����")}");
            Debug.Log($"���� ��: {isTransforming}");
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // ť�� ���� �̸�����
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + cubeCenter, Vector3.one * cubeSize);

        // ť�� �߽���
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + cubeCenter, 0.2f);

        // Base �����̳� ǥ��
        if (baseContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(baseContainer.position, Vector3.one * 0.5f);
        }

        // ��� ������ ť�� ��ġ�� ǥ��
        if (availableCubePositions.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in availableCubePositions)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), 0.05f);
            }
        }

        // ���� �Ҵ�� ���� ��ġ�� ǥ��
        if (formationPositions.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var pos in formationPositions.Values)
            {
                Gizmos.DrawSphere(transform.TransformPoint(pos), 0.08f);
            }
        }

        // �������� ���� ��ġ ǥ��
        if (voxels.Count > 0)
        {
            Gizmos.color = isInCubeForm ? Color.blue : Color.white;
            foreach (var voxel in voxels)
            {
                if (voxel != null)
                {
                    Gizmos.DrawWireSphere(voxel.position, 0.03f);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // �׻� ǥ�õǴ� �⺻ ����
        if (baseContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, baseContainer.position);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// ���� ������ �ٽ� ���� (��Ÿ�ӿ� Base ������ ����� ���)
    /// </summary>
    public void RefreshVoxelCollection()
    {
        CollectVoxelsFromBase();
        CalculateAllCubePositions();
        AssignFormationPositions();

        Debug.Log($"[CubeTransformation] ���� �÷��� ���ΰ�ħ �Ϸ� - {voxels.Count}��");
    }

    /// <summary>
    /// Ư�� ������ ���� ��ȸ
    /// </summary>
    public VoxelInfo GetVoxelInfo(Transform voxel)
    {
        var info = new VoxelInfo();
        info.voxel = voxel;
        info.index = voxelIndices.ContainsKey(voxel) ? voxelIndices[voxel] : -1;
        info.originalPosition = originalPositions.ContainsKey(voxel) ? originalPositions[voxel] : Vector3.zero;
        info.formationPosition = formationPositions.ContainsKey(voxel) ? formationPositions[voxel] : Vector3.zero;
        info.isAssigned = formationPositions.ContainsKey(voxel);

        return info;
    }

    /// <summary>
    /// ���� ����� Ȯ�� (0~1)
    /// </summary>
    public float GetTransformationProgress()
    {
        if (!isTransforming) return isInCubeForm ? 1f : 0f;

        // �� ������ ��ǥ ��ġ ��� ���� ��ġ�� ����� ���
        float totalProgress = 0f;
        int validVoxels = 0;

        foreach (var voxel in voxels)
        {
            if (formationPositions.ContainsKey(voxel))
            {
                Vector3 start = originalPositions[voxel];
                Vector3 target = formationPositions[voxel];
                Vector3 current = voxel.localPosition;

                float distance = Vector3.Distance(start, target);
                if (distance > 0.01f)
                {
                    float currentDistance = Vector3.Distance(current, target);
                    float progress = 1f - (currentDistance / distance);
                    totalProgress += Mathf.Clamp01(progress);
                    validVoxels++;
                }
            }
        }

        return validVoxels > 0 ? totalProgress / validVoxels : 0f;
    }

    /// <summary>
    /// ��� ���� (��� Ʈ�� �ߴ�)
    /// </summary>
    public void EmergencyStop()
    {
        DOTween.KillAll();
        isTransforming = false;

        Debug.LogWarning("[CubeTransformation] ��� ���� ����!");
    }

    #endregion

    #region Data Structures

    [System.Serializable]
    public struct VoxelInfo
    {
        public Transform voxel;
        public int index;
        public Vector3 originalPosition;
        public Vector3 formationPosition;
        public bool isAssigned;
    }

    #endregion
}