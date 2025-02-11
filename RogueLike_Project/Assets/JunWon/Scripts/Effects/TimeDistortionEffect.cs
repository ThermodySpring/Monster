using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TimeDistortionEffect : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("���� �� �ð� ������ ��")]
    public float slowdownFactor = 0.05f;
    [Tooltip("��ü ���� ���� �ð� (��)")]
    public float slowdownDuration = 0.3f;
    [Tooltip("���� �� �ִϸ��̼� Ŀ�� (0: ���ӻ���, 1: ����)")]
    public AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    public PostProcessVolume postProcessVolume;
    [Tooltip("�ִ� ũ�θ�ƽ ����̼� ����")]
    public float maxChromaticIntensity = 1f;
    [Tooltip("�ִ� ��� �� ���� �ޱ�")]
    public float maxMotionBlurShutterAngle = 270f;
    [Tooltip("�ִ� ��� �� ���� ��")]
    public int maxMotionBlurSampleCount = 10;

    private ChromaticAberration chromaticAberration;
    private MotionBlur motionBlur;

    private float defaultFixedDeltaTime;
    private Coroutine currentEffect;

    void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out motionBlur);
        }
        else
        {
            Debug.LogWarning("PostProcessVolume �Ǵ� �������� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ܺο��� �� �Լ��� ȣ���ϸ� Ÿ�� ����� ȿ���� Ʈ�����մϴ�.
    /// </summary>
    public void TriggerEffect()
    {
        // �̹� ȿ�� ���� ���̸� �ߴ� �� �� ȿ�� ����
        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
        }
        currentEffect = StartCoroutine(TimeDistortionSequence());
    }

    IEnumerator TimeDistortionSequence()
    {
        // --- ���� �� ȿ�� ���� ---
        // ��� �ð� ����
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowdownFactor;

        // ����Ʈ ���μ��� ȿ�� ����
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = maxChromaticIntensity;
        }
        if (motionBlur != null)
        {
            motionBlur.shutterAngle.value = maxMotionBlurShutterAngle;
            motionBlur.sampleCount.value = maxMotionBlurSampleCount;
        }

        // ���� ���¸� ��� ���� (��ü �ð��� ����)
        yield return new WaitForSecondsRealtime(slowdownDuration * 0.5f);

        // --- ���� �ܰ�: �ð� �� �ð� ȿ�� �ε巴�� ���� ---
        float elapsed = 0f;
        float recoveryDuration = slowdownDuration * 0.5f;
        while (elapsed < recoveryDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / recoveryDuration);
            float curveValue = recoveryCurve.Evaluate(t);

            // �ð� ������ ����
            Time.timeScale = Mathf.Lerp(slowdownFactor, 1f, curveValue);
            Time.fixedDeltaTime = Mathf.Lerp(defaultFixedDeltaTime * slowdownFactor, defaultFixedDeltaTime, curveValue);

            // ũ�θ�ƽ ����̼� ����
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(maxChromaticIntensity, 0f, curveValue);
            }
            // ��� ���� �ڿ������� ���� (���ϴ� ���)
            if (motionBlur != null)
            {
                motionBlur.shutterAngle.value = Mathf.Lerp(maxMotionBlurShutterAngle, 0f, curveValue);
                // sampleCount�� �����̹Ƿ� ���� �� �ݿø� ó��
                motionBlur.sampleCount.value = Mathf.RoundToInt(Mathf.Lerp(maxMotionBlurSampleCount, 0, curveValue));
            }

            yield return null;
        }

        // --- ���� �� ���� ---
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }
        if (motionBlur != null)
        {
            motionBlur.shutterAngle.value = 0f;
            motionBlur.sampleCount.value = 0;
        }

        currentEffect = null;
    }

    void OnDisable()
    {
        // ��ũ��Ʈ�� ��Ȱ��ȭ�� �� �����ϰ� �ð� ������ ����
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
