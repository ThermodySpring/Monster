using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataReaperBasicEffect : MonoBehaviour
{
    [Header("�����ڸ� ȿ�� ����")]
    [SerializeField] private float edgeWidth = 0.02f;
    [SerializeField] private float edgeGlowIntensity = 1.5f;
    [SerializeField] private Color edgeColor = Color.cyan;

    [Header("������ ȿ�� ����")]
    [Range(0, 1)]
    [SerializeField] private float glitchIntensity = 0.2f;
    [SerializeField] private float pulseSpeed = 1.0f;
    [SerializeField] private float noiseScale = 5.0f;

    private Renderer[] renderers;
    private float time;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // ��� �������� ��Ƽ���� �ʱⰪ ����
        UpdateShaderProperties();
    }

    void Update()
    {
        time += Time.deltaTime * pulseSpeed;

        // �����ڸ� ȿ�� �Ķ���� ������Ʈ
        float pulseFactor = Mathf.Sin(time) * 0.5f + 0.5f; // 0~1 ������ �Ƶ���
        float currentEdgeWidth = edgeWidth * (0.8f + pulseFactor * 0.4f);
        float currentGlowIntensity = edgeGlowIntensity * (0.9f + pulseFactor * 0.2f);

        // �������̳� �ִϸ��̼� ���¿� ���� ������ ���⼭ ����

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_EdgeWidth"))
                    mat.SetFloat("_EdgeWidth", currentEdgeWidth);

                if (mat.HasProperty("_EdgeGlowIntensity"))
                    mat.SetFloat("_EdgeGlowIntensity", currentGlowIntensity);

                if (mat.HasProperty("_GlitchIntensity"))
                    mat.SetFloat("_GlitchIntensity", glitchIntensity);

                if (mat.HasProperty("_Time"))
                    mat.SetFloat("_Time", time);
            }
        }
    }

    // ȿ�� ���� �Ͻ��� ���� �Լ� (�ִϸ��̼� �̺�Ʈ ��� ȣ��)
    public void PulseEffect(float intensity = 1.0f)
    {
        StartCoroutine(PulseCoroutine(intensity));
    }

    private System.Collections.IEnumerator PulseCoroutine(float intensity)
    {
        float originalGlitch = glitchIntensity;
        glitchIntensity += intensity * 0.3f;
        glitchIntensity = Mathf.Clamp01(glitchIntensity);

        UpdateShaderProperties();

        yield return new WaitForSeconds(0.2f);

        glitchIntensity = originalGlitch;
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty("_EdgeColor"))
                    mat.SetColor("_EdgeColor", edgeColor);

                if (mat.HasProperty("_EdgeWidth"))
                    mat.SetFloat("_EdgeWidth", edgeWidth);

                if (mat.HasProperty("_EdgeGlowIntensity"))
                    mat.SetFloat("_EdgeGlowIntensity", edgeGlowIntensity);

                if (mat.HasProperty("_GlitchIntensity"))
                    mat.SetFloat("_GlitchIntensity", glitchIntensity);

                if (mat.HasProperty("_NoiseScale"))
                    mat.SetFloat("_NoiseScale", noiseScale);
            }
        }
    }
}
