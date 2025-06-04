using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpreadPattern
{
    Uniform,        // �յ��ϰ� ����
    Explosive,      // ���������� ����
    Wave,          // �ĵ�ó�� ����
    Spiral,        // ���������� ����
    Random         // �����ϰ� ����
}

public class VirusCubeSpread : MonoBehaviour
{
    [Header("Spread Pattern Settings")]
    [SerializeField] private SpreadPattern spreadPattern = SpreadPattern.Explosive;
    [SerializeField] private float spreadIntensity = 1.5f;
    [SerializeField] private bool addRandomOffset = true;
    [SerializeField] private float randomOffsetAmount = 0.3f;

    /// <summary>
    /// ���Ͽ� ���� ���� ��ġ ���
    /// </summary>
    public Vector3 CalculateSpreadPosition(Vector3 cubePos, int index, int totalCount)
    {
        Vector3 baseDirection = cubePos.normalized;
        Vector3 spreadPos = Vector3.zero;

        switch (spreadPattern)
        {
            case SpreadPattern.Uniform:
                spreadPos = UniformSpread(baseDirection);
                break;

            case SpreadPattern.Explosive:
                spreadPos = ExplosiveSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Wave:
                spreadPos = WaveSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Spiral:
                spreadPos = SpiralSpread(baseDirection, index, totalCount);
                break;

            case SpreadPattern.Random:
                spreadPos = RandomSpread(baseDirection);
                break;
        }

        // ���� ������ �߰�
        if (addRandomOffset)
        {
            Vector3 randomOffset = Random.insideUnitSphere * randomOffsetAmount;
            spreadPos += randomOffset;
        }

        return spreadPos;
    }

    private Vector3 UniformSpread(Vector3 direction)
    {
        return direction * spreadIntensity * 2.5f;
    }

    private Vector3 ExplosiveSpread(Vector3 direction, int index, int totalCount)
    {
        // �߽ɿ��� �ּ��� �� �ָ� ����
        float distanceFromCenter = direction.magnitude;
        float explosiveForce = 1f + (distanceFromCenter * 2f);
        return direction * spreadIntensity * explosiveForce * 2.5f;
    }

    private Vector3 WaveSpread(Vector3 direction, int index, int totalCount)
    {
        // �ð����� �ΰ� �ĵ�ó�� ����
        float wave = Mathf.Sin((index / (float)totalCount) * Mathf.PI * 2f + Time.time * 4f);
        float waveForce = 1f + wave * 0.5f;
        return direction * spreadIntensity * waveForce * 2.5f;
    }

    private Vector3 SpiralSpread(Vector3 direction, int index, int totalCount)
    {
        // ���������� ����
        float angle = (index / (float)totalCount) * Mathf.PI * 4f; // 2���� ����
        Vector3 spiralOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.5f;
        return (direction + spiralOffset) * spreadIntensity * 2.5f;
    }

    private Vector3 RandomSpread(Vector3 direction)
    {
        // ���� �����ϰ� ����
        Vector3 randomDir = Random.insideUnitSphere.normalized;
        Vector3 blendedDirection = Vector3.Lerp(direction, randomDir, 0.3f);
        return blendedDirection * spreadIntensity * Random.Range(1.5f, 3.5f);
    }
}
