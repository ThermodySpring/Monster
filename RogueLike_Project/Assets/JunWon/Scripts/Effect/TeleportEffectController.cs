// TeleportEffectController.cs
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TeleportEffectController : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private Color edgeColor1 = new Color(0f, 1f, 0.8f, 1f);
    [SerializeField] private Color edgeColor2 = new Color(0f, 0.7f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;
    [SerializeField] private float glitchIntensity = 0.1f;

    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private Renderer[] renderers;
    private bool isDissolving = false;

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        dissolveMaterials = new Material[renderers.Length];

        // ������ ȿ���� ��Ƽ������ �̸� ������ ���� ������
        Material dissolveEffectMaterial = Resources.Load<Material>("Materials/DigitalDissolveEffect");

        for (int i = 0; i < renderers.Length; i++)
        {
            // ���� Lit ��Ƽ���� ����
            originalMaterials[i] = renderers[i].material;

            // ������ ��Ƽ���� ����
            dissolveMaterials[i] = new Material(dissolveEffectMaterial);

            // ���� Lit ��Ƽ������ ���� �����ͼ� ����
            if (originalMaterials[i].HasProperty("_BaseColor")) // URP
            {
                Color originalColor = originalMaterials[i].GetColor("_BaseColor");
                dissolveMaterials[i].SetColor("_EdgeColor1",
                    new Color(edgeColor1.r * originalColor.r,
                             edgeColor1.g * originalColor.g,
                             edgeColor1.b * originalColor.b,
                             edgeColor1.a));
                dissolveMaterials[i].SetColor("_EdgeColor2",
                    new Color(edgeColor2.r * originalColor.r,
                             edgeColor2.g * originalColor.g,
                             edgeColor2.b * originalColor.b,
                             edgeColor2.a));
            }
            else if (originalMaterials[i].HasProperty("_Color")) // Built-in
            {
                Color originalColor = originalMaterials[i].GetColor("_Color");
                dissolveMaterials[i].SetColor("_EdgeColor1",
                    new Color(edgeColor1.r * originalColor.r,
                             edgeColor1.g * originalColor.g,
                             edgeColor1.b * originalColor.b,
                             edgeColor1.a));
                dissolveMaterials[i].SetColor("_EdgeColor2",
                    new Color(edgeColor2.r * originalColor.r,
                             edgeColor2.g * originalColor.g,
                             edgeColor2.b * originalColor.b,
                             edgeColor2.a));
            }

            SetupDissolveMaterial(dissolveMaterials[i]);
        }
    }

    private void SetupDissolveMaterial(Material mat)
    {
        mat.SetFloat("_DissolveAmount", 0);
        mat.SetColor("_EdgeColor1", edgeColor1);
        mat.SetColor("_EdgeColor2", edgeColor2);
        mat.SetFloat("_EmissionIntensity", emissionIntensity);
        mat.SetFloat("_GlitchIntensity", glitchIntensity);
    }

    public void StartDissolve()
    {
        if (!isDissolving)
        {
            isDissolving = true;
            // ������ ��Ƽ����� ��ü
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = dissolveMaterials[i];
                dissolveMaterials[i].SetFloat("_DissolveAmount", 1);
                dissolveMaterials[i].SetFloat("_GlitchIntensity", glitchIntensity * 2f);
                dissolveMaterials[i].SetFloat("_EmissionIntensity", emissionIntensity * 2f);
            }
        }
    }

    public void EndDissolve()
    {
        if (isDissolving)
        {
            isDissolving = false;
            // ���� ��Ƽ����� ����
            for (int i = 0; i < renderers.Length; i++)
            {
                dissolveMaterials[i].SetFloat("_DissolveAmount", 0);
                dissolveMaterials[i].SetFloat("_GlitchIntensity", glitchIntensity);
                dissolveMaterials[i].SetFloat("_EmissionIntensity", emissionIntensity);
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    private void OnDestroy()
    {
        // ������ ������ ��Ƽ���� ����
        if (dissolveMaterials != null)
        {
            foreach (Material mat in dissolveMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }

    private void SetDissolveAmount(float amount)
    {
        foreach (Material mat in dissolveMaterials)
        {
            mat.SetFloat("_DissolveAmount", amount);
            mat.SetFloat("_GlitchIntensity", glitchIntensity * (amount > 0 ? 2f : 1f));
            mat.SetFloat("_EmissionIntensity", emissionIntensity * (amount > 0 ? 2f : 1f));
        }
    }


#if UNITY_EDITOR
    // ����׿� �޼����
    public void TestDissolveEffect()
    {
        Debug.Log("Testing Dissolve Effect");
        StartDissolve();
        Invoke(nameof(EndDissolve), 1f);
    }

    public void TestRandomTeleport()
    {
        Debug.Log("Testing Random Teleport");
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        StartDissolve();
        Invoke(nameof(DebugTeleport), 0.5f);
    }

    private void DebugTeleport()
    {
        EndDissolve();
    }

    public void ResetEffect()
    {
        Debug.Log("Resetting Effect");
        CancelInvoke();
        isDissolving = false;
        SetDissolveAmount(0);
    }

    private void OnDrawGizmos()
    {
    }
#endif

}

