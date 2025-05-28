using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelFloatEffect : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatAmplitude = 0.3f;    // ���ٴϴ� ����
    public float floatSpeed = 1f;          // ���ٴϴ� �ӵ�
    public float randomOffset = 0.2f;      // �� �������� �ٸ� ������

    // �������� ���� ��ġ ����
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, float> voxelOffsets = new Dictionary<Transform, float>();

    void Start()
    {
        // �ڽ� ��ü��(������)�� ���� ��ġ ����
        InitializeVoxelPositions();
    }

    void Update()
    {
        FloatVoxels();
    }

    private void InitializeVoxelPositions()
    {
        // ��� �ڽ� �������� ���� ��ġ�� ���� ������ ����
        foreach (Transform child in transform)
        {
            originalPositions[child] = child.localPosition;
            voxelOffsets[child] = Random.Range(0f, 2f * Mathf.PI); // ���� �ٸ� ������
        }
    }

    private void FloatVoxels()
    {
        foreach (Transform voxel in originalPositions.Keys)
        {
            if (voxel == null) continue;

            // �� �������� �ٸ� �������� ���ٴϰ�
            float timeOffset = voxelOffsets[voxel];

            // Y�� ���� ������
            float floatY = Mathf.Sin((Time.time * floatSpeed) + timeOffset) * floatAmplitude;

            // X, Z�൵ ��¦ ������ (�� �ڿ�������)
            float floatX = Mathf.Cos((Time.time * floatSpeed * 0.7f) + timeOffset) * (floatAmplitude * 0.3f);
            float floatZ = Mathf.Sin((Time.time * floatSpeed * 0.5f) + timeOffset) * (floatAmplitude * 0.3f);

            // ���� ������ �߰�
            Vector3 randomFloat = new Vector3(floatX, floatY, floatZ) * randomOffset;

            // ���� ��ġ���� ���ٴϴ� ȿ�� ����
            Vector3 targetPosition = originalPositions[voxel] + randomFloat;
            voxel.localPosition = targetPosition;
        }
    }

    // ���ٴϴ� ���� ���� (�ܺο��� ȣ�� ����)
    public void SetFloatIntensity(float intensity)
    {
        floatAmplitude = 0.3f * intensity;
        floatSpeed = 1f * intensity;
        randomOffset = 0.2f * intensity;
    }
}