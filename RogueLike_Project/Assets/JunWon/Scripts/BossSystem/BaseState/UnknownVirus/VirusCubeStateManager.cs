using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusCubeStateManager : MonoBehaviour
{
    [System.Serializable]
    public class VoxelState
    {
        public Transform voxel;
        public Vector3 originalPosition;
        public Vector3 originalScale;
        public Quaternion originalRotation;
        public Color originalColor;
        public Material originalMaterial;

        public VoxelState(Transform voxelTransform)
        {
            voxel = voxelTransform;
            originalPosition = voxelTransform.localPosition;
            originalScale = voxelTransform.localScale;
            originalRotation = voxelTransform.localRotation;

            Renderer renderer = voxelTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material;
            }
            else
            {
                originalColor = Color.white;
            }
        }
    }

    [Header("State Management")]
    [SerializeField] private List<VoxelState> voxelStates = new List<VoxelState>();
    [SerializeField] private bool autoSaveOnStart = true;

    private void Start()
    {
        if (autoSaveOnStart)
        {
            SaveCurrentState();
        }
    }

    /// <summary>
    /// ���� ���¸� ���� ���·� ����
    /// </summary>
    public void SaveCurrentState()
    {
        voxelStates.Clear();

        foreach (Transform child in transform)
        {
            VoxelState state = new VoxelState(child);
            voxelStates.Add(state);
        }

        Debug.Log($"[VirusCubeStateManager] {voxelStates.Count}�� ���� ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ���� ���·� ����
    /// </summary>
    public void RestoreOriginalState()
    {
        foreach (VoxelState state in voxelStates)
        {
            if (state.voxel != null)
            {
                // ��ġ, ũ��, ȸ�� ����
                state.voxel.localPosition = state.originalPosition;
                state.voxel.localScale = state.originalScale;
                state.voxel.localRotation = state.originalRotation;

                // ���� �� ��Ƽ���� ����
                Renderer renderer = state.voxel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (state.originalMaterial != null)
                    {
                        renderer.material = state.originalMaterial;
                    }
                    else
                    {
                        renderer.material.color = state.originalColor;
                    }
                }
            }
        }

        Debug.Log("[VirusCubeStateManager] ���� ���� ���� �Ϸ�");
    }

    /// <summary>
    /// �ε巴�� ���� ���·� ����
    /// </summary>
    public IEnumerator RestoreOriginalStateSmooth(float duration = 1.5f)
    {
        float elapsed = 0f;

        // ���� ���� ����
        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> startScales = new List<Vector3>();
        List<Quaternion> startRotations = new List<Quaternion>();
        List<Color> startColors = new List<Color>();

        foreach (VoxelState state in voxelStates)
        {
            if (state.voxel != null)
            {
                startPositions.Add(state.voxel.localPosition);
                startScales.Add(state.voxel.localScale);
                startRotations.Add(state.voxel.localRotation);

                Renderer renderer = state.voxel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    startColors.Add(renderer.material.color);
                }
                else
                {
                    startColors.Add(Color.white);
                }
            }
        }

        // �ε巴�� ����
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < voxelStates.Count; i++)
            {
                VoxelState state = voxelStates[i];
                if (state.voxel != null && i < startPositions.Count)
                {
                    // ��ġ ����
                    state.voxel.localPosition = Vector3.Lerp(startPositions[i], state.originalPosition, easedProgress);

                    // ũ�� ����
                    state.voxel.localScale = Vector3.Lerp(startScales[i], state.originalScale, easedProgress);

                    // ȸ�� ����
                    state.voxel.localRotation = Quaternion.Lerp(startRotations[i], state.originalRotation, easedProgress);

                    // ���� ����
                    Renderer renderer = state.voxel.GetComponent<Renderer>();
                    if (renderer != null && i < startColors.Count)
                    {
                        Color lerpedColor = Color.Lerp(startColors[i], state.originalColor, easedProgress);
                        renderer.material.color = lerpedColor;
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ���� ��Ȯ�� ����
        RestoreOriginalState();
    }

    /// <summary>
    /// Ư�� ������ ���� ���� ��������
    /// </summary>
    public VoxelState GetVoxelState(Transform voxel)
    {
        foreach (VoxelState state in voxelStates)
        {
            if (state.voxel == voxel)
            {
                return state;
            }
        }
        return null;
    }

    /// <summary>
    /// ��� ���� ���� ��������
    /// </summary>
    public List<VoxelState> GetAllVoxelStates()
    {
        return new List<VoxelState>(voxelStates);
    }
}
