using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ȭ�� ���� �÷��� ȿ�� - ���̷��� ť���
/// </summary>
public class VoxelFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // ���ٴϴ� ����
    public float floatSpeed = 1f;          // ���ٴϴ� �ӵ�
    public float randomOffset = 0.2f;      // �� �������� �ٸ� ������

    [Header("Advanced Effects")]
    public bool enableOrbitalMotion = true;        // �˵� � Ȱ��ȭ
    public float orbitalRadius = 0.1f;             // �˵� ������
    public float orbitalSpeed = 2f;                // �˵� �ӵ�
    public bool enablePulseEffect = true;          // �޽� ȿ��
    public float pulseIntensity = 0.05f;           // �޽� ����
    public bool enableGlitchFloat = false;         // �۸�ġ �÷���
    public float glitchChance = 0.1f;              // �۸�ġ �߻� Ȯ��
    public float glitchDuration = 0.2f;            // �۸�ġ ���� �ð�

    [Header("Virus Effects")]
    public bool enableVirusCorruption = true;     // ���̷��� ���� ȿ��
    public float corruptionIntensity = 0.15f;     // ���� ����
    public float corruptionSpeed = 3f;            // ���� �ӵ�

    // �������� ���� ��ġ ����
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> voxelOffsets = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> orbitalOffsets = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> glitchTimers = new Dictionary<Transform, float>();
    private Dictionary<Transform, Vector3> glitchTargets = new Dictionary<Transform, Vector3>();

    // ȿ�� ���� ����
    private float globalIntensityMultiplier = 1f;
    private bool isPaused = false;

    void Start()
    {
        // �ڽ� ��ü��(������)�� ���� ��ġ ����
        InitializeVoxelPositions();
    }

    void Update()
    {
        if (!isPaused)
        {
            FloatVoxels();
        }
    }

    private void InitializeVoxelPositions()
    {
        // ��� �ڽ� �������� ���� ��ġ�� ���� ������ ����
        foreach (Transform child in transform)
        {
            originalPositions[child] = child.localPosition;
            voxelOffsets[child] = Random.Range(0f, 2f * Mathf.PI); // ���� �ٸ� ������

            // �˵� ��� ���� ���� ������
            orbitalOffsets[child] = Random.insideUnitSphere * 0.1f;

            // �۸�ġ Ÿ�̸� �ʱ�ȭ
            glitchTimers[child] = 0f;
            glitchTargets[child] = Vector3.zero;
        }
    }

    private void FloatVoxels()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // �� �������� �ٸ� �������� ���ٴϰ�
            float timeOffset = voxelOffsets[voxel];
            Vector3 finalOffset = Vector3.zero;

            // 1. �⺻ �÷��� ȿ��
            finalOffset += CalculateBasicFloat(timeOffset);

            // 2. �˵� � ȿ��
            if (enableOrbitalMotion)
            {
                finalOffset += CalculateOrbitalMotion(voxel, timeOffset);
            }

            // 3. �޽� ȿ��
            if (enablePulseEffect)
            {
                finalOffset += CalculatePulseEffect(timeOffset);
            }

            // 4. �۸�ġ ȿ��
            if (enableGlitchFloat)
            {
                finalOffset += CalculateGlitchEffect(voxel);
            }

            // 5. ���̷��� ���� ȿ��
            if (enableVirusCorruption)
            {
                finalOffset += CalculateVirusCorruption(voxel, timeOffset);
            }

            // �۷ι� ���� ����
            finalOffset *= globalIntensityMultiplier;

            // ���� ��ġ ����
            Vector3 targetPosition = originalPositions[voxel] + finalOffset;
            voxel.localPosition = Vector3.Lerp(voxel.localPosition, targetPosition, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// �⺻ �÷��� ���
    /// </summary>
    private Vector3 CalculateBasicFloat(float timeOffset)
    {
        // Y�� ���� ������
        float floatY = Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;

        // X, Z�൵ ��¦ ������ (�� �ڿ�������)
        float floatX = Mathf.Cos((Time.time * floatSpeed * 0.7f) + timeOffset) * (floatAmplitude * 0.3f);
        float floatZ = Mathf.Sin((Time.time * floatSpeed * 0.5f) + timeOffset) * (floatAmplitude * 0.3f);

        return new Vector3(floatX, floatY, floatZ) * randomOffset;
    }

    /// <summary>
    /// �˵� � ���
    /// </summary>
    private Vector3 CalculateOrbitalMotion(Transform voxel, float timeOffset)
    {
        Vector3 orbitalBase = orbitalOffsets[voxel];
        float orbitalTime = Time.time * orbitalSpeed + timeOffset;

        Vector3 orbital = new Vector3(
            Mathf.Cos(orbitalTime) * orbitalRadius,
            Mathf.Sin(orbitalTime * 1.3f) * orbitalRadius * 0.5f,
            Mathf.Sin(orbitalTime) * orbitalRadius
        );

        return orbital + orbitalBase;
    }

    /// <summary>
    /// �޽� ȿ�� ���
    /// </summary>
    private Vector3 CalculatePulseEffect(float timeOffset)
    {
        float pulse = Mathf.Sin(Time.time * 4f + timeOffset) * pulseIntensity;
        return Vector3.one * pulse;
    }

    /// <summary>
    /// �۸�ġ ȿ�� ���
    /// </summary>
    private Vector3 CalculateGlitchEffect(Transform voxel)
    {
        // �۸�ġ Ÿ�̸� ������Ʈ
        glitchTimers[voxel] -= Time.deltaTime;

        // ���ο� �۸�ġ ���� üũ
        if (glitchTimers[voxel] <= 0f && Random.value < glitchChance * Time.deltaTime)
        {
            glitchTimers[voxel] = glitchDuration;
            glitchTargets[voxel] = Random.insideUnitSphere * 0.3f;
        }

        // �۸�ġ ȿ�� ����
        if (glitchTimers[voxel] > 0f)
        {
            float intensity = glitchTimers[voxel] / glitchDuration;
            return glitchTargets[voxel] * intensity;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// ���̷��� ���� ȿ�� ���
    /// </summary>
    private Vector3 CalculateVirusCorruption(Transform voxel, float timeOffset)
    {
        float corruptionTime = Time.time * corruptionSpeed + timeOffset;

        // �ұ�Ģ�� ������ �ְ�
        Vector3 corruption = new Vector3(
            Mathf.PerlinNoise(corruptionTime, 0f) - 0.5f,
            Mathf.PerlinNoise(0f, corruptionTime) - 0.5f,
            Mathf.PerlinNoise(corruptionTime, corruptionTime) - 0.5f
        );

        return corruption * corruptionIntensity;
    }

    // ���ٴϴ� ���� ���� (�ܺο��� ȣ�� ����)
    public void SetFloatIntensity(float intensity)
    {
        globalIntensityMultiplier = intensity;
        floatAmplitude = 0.3f * intensity;
        floatSpeed = 1f * intensity;
        randomOffset = 0.2f * intensity;
    }

    /// <summary>
    /// �۸�ġ ��� ���
    /// </summary>
    public void SetGlitchMode(bool enabled, float intensity = 1f)
    {
        enableGlitchFloat = enabled;
        if (enabled)
        {
            glitchChance = 0.1f * intensity;
            glitchDuration = 0.2f / intensity;
        }
    }

    /// <summary>
    /// ���̷��� ���� ��� ����
    /// </summary>
    public void SetVirusCorruption(bool enabled, float intensity = 1f)
    {
        enableVirusCorruption = enabled;
        corruptionIntensity = 0.15f * intensity;
        corruptionSpeed = 3f * intensity;
    }

    /// <summary>
    /// �˵� � ����
    /// </summary>
    public void SetOrbitalMotion(bool enabled, float radius = 0.1f, float speed = 2f)
    {
        enableOrbitalMotion = enabled;
        orbitalRadius = radius;
        orbitalSpeed = speed;
    }

    /// <summary>
    /// ��� ȿ�� �Ͻ� ����/�簳
    /// </summary>
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    /// <summary>
    /// ��¡ ���� ���� (���� �غ��)
    /// </summary>
    public void SetChargingMode(bool charging)
    {
        if (charging)
        {
            SetFloatIntensity(2f);
            SetGlitchMode(true, 2f);
            enablePulseEffect = true;
            pulseIntensity = 0.1f;
        }
        else
        {
            SetFloatIntensity(1f);
            SetGlitchMode(false);
            enablePulseEffect = true;
            pulseIntensity = 0.05f;
        }
    }

    /// <summary>
    /// ���� ��� ����
    /// </summary>
    public void SetAttackMode(bool attacking)
    {
        if (attacking)
        {
            SetFloatIntensity(0.2f); // ���ݽÿ��� ���ٴϴ� ȿ�� �ּ�ȭ
            SetGlitchMode(false);
            enableOrbitalMotion = false;
        }
        else
        {
            SetFloatIntensity(1f);
            enableOrbitalMotion = true;
        }
    }

    /// <summary>
    /// �Ҹ� ��� ����
    /// </summary>
    public void SetDissolveMode(bool dissolving)
    {
        if (dissolving)
        {
            SetFloatIntensity(3f);
            SetGlitchMode(true, 3f);
            SetVirusCorruption(true, 2f);
        }
    }

    /// <summary>
    /// ��� ������ ���� ��ġ�� ����
    /// </summary>
    public void ResetToOriginalPositions()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel != null)
            {
                voxel.localPosition = originalPositions[voxel];
            }
        }
    }

    /// <summary>
    /// ���� �߰��� �ڽ� �������� �ʱ�ȭ
    /// </summary>
    public void RefreshVoxelList()
    {
        // ���� ������ Ŭ����
        originalPositions.Clear();
        voxelOffsets.Clear();
        orbitalOffsets.Clear();
        glitchTimers.Clear();
        glitchTargets.Clear();

        // ���ʱ�ȭ
        InitializeVoxelPositions();
    }

    /// <summary>
    /// ����׿� �����
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (originalPositions.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in originalPositions.Values)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(pos), 0.05f);
            }
        }
    }
}