using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    Volume volume;
    Vignette vignette;
    ChromaticAberration chromatic;
    bool isCritical = false;

    // �۸�ġ ȿ�� ���� ����
    private Coroutine glitchCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        Vignette tmp;
        if (volume.profile.TryGet<Vignette>(out tmp))
        {
            vignette = tmp;
        }
        ChromaticAberration tmp1;
        if (volume.profile.TryGet<ChromaticAberration>(out tmp1))
        {
            chromatic = tmp1;
        }
        StartCoroutine(VignetteAnimation());
    }

    IEnumerator VignetteAnimation()
    {
        while (true)
        {
            if (isCritical)
            {
                while (vignette.intensity.value < 0.4f)
                {
                    vignette.intensity.value += Time.deltaTime * 0.2f;
                    yield return null;
                }
                while (vignette.intensity.value > 0.2f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.2f;
                    yield return null;
                }
            }
            else
            {
                while (vignette.intensity.value > 0f)
                {
                    vignette.intensity.value -= Time.deltaTime * 0.3f;
                    yield return null;
                }
            }
            yield return null;
        }
    }

    public void ChangeVignetteColor(Color color)
    {
        vignette.color.value = color;
    }

    public void ChangeChromaticAberrationActive(bool active)
    {
        chromatic.active = active;
        isCritical = active;
    }

    public void DamagedEffect(float intensity)
    {
        if (vignette.color.value == Color.white)
        {
            vignette.color.value = Color.red;
        }
        if (vignette.intensity.value <= 0.02f) vignette.intensity.value = 0.3f;
        if (vignette.intensity.value <= 0.6f) vignette.intensity.value += intensity;
    }

    public void DamagedEffect(float intensity, Color color)
    {
        if (vignette.color.value == Color.white)
        {
            vignette.color.value = color;
        }
        if (vignette.intensity.value <= 0.02f) vignette.intensity.value = 0.3f;
        if (vignette.intensity.value <= 0.6f) vignette.intensity.value += intensity;
    }

    // ���� �߰��� �۸�ġ ȿ�� ���� �Լ�
    public void EnableGlitchEffect(float intensity)
    {
        // ũ�θ�ƽ �ֹ����̼� ���� ����
        if (chromatic != null)
        {
            chromatic.active = true;
            chromatic.intensity.value = Mathf.Clamp01(intensity);
        }
    }

    public void DisableGlitchEffect()
    {
        // ���� ���� �۸�ġ ȿ���� �ִٸ� ����
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }

        // ũ��Ƽ�� ���°� �ƴϸ� ũ�θ�ƽ �ֹ����̼� ��Ȱ��ȭ
        if (chromatic != null && !isCritical)
        {
            chromatic.active = false;
            chromatic.intensity.value = 0f;
        }
    }

    public void TriggerGlitchEffect(float duration)
    {
        // ������ ���� ���� �۸�ġ �ڷ�ƾ�� �ִٸ� ����
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
        }

        // ���ο� �۸�ġ ȿ�� �ڷ�ƾ ����
        glitchCoroutine = StartCoroutine(GlitchEffectRoutine(duration));
    }

    private IEnumerator GlitchEffectRoutine(float duration)
    {
        if (chromatic == null) yield break;

        // ���� ũ�θ�ƽ �ֹ����̼� ���� ����
        bool originalActive = chromatic.active;
        float originalIntensity = chromatic.intensity.value;

        // �۸�ġ ȿ�� Ȱ��ȭ
        chromatic.active = true;

        // �۸�ġ ȿ�� ���� ���� (0.2��)
        float startTime = Time.time;
        float riseTime = 0.2f;

        while (Time.time - startTime < riseTime)
        {
            float t = (Time.time - startTime) / riseTime;
            chromatic.intensity.value = Mathf.Lerp(originalIntensity, 1.0f, t);
            yield return null;
        }

        // �ִ� ������ ����
        chromatic.intensity.value = 1.0f;

        // ���� �ð��� 60% ���� ����
        float holdTime = duration * 0.6f - riseTime;
        if (holdTime > 0)
            yield return new WaitForSeconds(holdTime);

        // �ٽ� ���� ���·� ���� (0.2��)
        startTime = Time.time;
        float fallTime = 0.2f;

        while (Time.time - startTime < fallTime)
        {
            float t = (Time.time - startTime) / fallTime;
            chromatic.intensity.value = Mathf.Lerp(1.0f, originalIntensity, t);
            yield return null;
        }

        // ���� ���·� ���� (isCritical�� ����)
        if (!isCritical)
        {
            chromatic.active = originalActive;
        }
        chromatic.intensity.value = originalIntensity;

        glitchCoroutine = null;
    }
}