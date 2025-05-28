// 1. Editor ������ ���� ������ ��ũ��Ʈ
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class VoxelBossEditorWindow : EditorWindow
{
    private VoxelGenerator selectedGenerator;
    private BossType selectedBossType = BossType.Ransomware;
    private Material voxelMaterial;
    private Material glitchMaterial;
    private float voxelSize = 1f;
    private Vector3 bossSize = new Vector3(8, 8, 8);

    // �̸������
    private bool showPreview = true;
    private Color previewColor = Color.white;

    [MenuItem("Tools/Voxel Boss Generator")]
    public static void ShowWindow()
    {
        VoxelBossEditorWindow window = GetWindow<VoxelBossEditorWindow>("���� ���� ������");
        window.minSize = new Vector2(300, 400);
    }

    void OnGUI()
    {
        GUILayout.Label("���� ���� ���� ����", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // �⺻ ����
        GUILayout.Label("�⺻ ����", EditorStyles.boldLabel);
        selectedGenerator = (VoxelGenerator)EditorGUILayout.ObjectField("���� ������", selectedGenerator, typeof(VoxelGenerator), true);

        if (selectedGenerator == null)
        {
            EditorGUILayout.HelpBox("Scene�� VoxelGenerator�� �ִ� ������Ʈ�� �����ϼ���.", MessageType.Warning);

            if (GUILayout.Button("�� ���� ������ �����"))
            {
                CreateNewVoxelGenerator();
            }
        }

        EditorGUILayout.Space();

        // ���� Ÿ�� ����
        GUILayout.Label("���� Ÿ��", EditorStyles.boldLabel);
        selectedBossType = (BossType)EditorGUILayout.EnumPopup("���� ����", selectedBossType);

        EditorGUILayout.Space();

        // ���� ����
        GUILayout.Label("���� ����", EditorStyles.boldLabel);
        voxelSize = EditorGUILayout.FloatField("���� ũ��", voxelSize);
        bossSize = EditorGUILayout.Vector3Field("���� ũ��", bossSize);

        voxelMaterial = (Material)EditorGUILayout.ObjectField("�⺻ ��Ƽ����", voxelMaterial, typeof(Material), false);
        glitchMaterial = (Material)EditorGUILayout.ObjectField("�۸�ġ ��Ƽ����", glitchMaterial, typeof(Material), false);

        EditorGUILayout.Space();

        // �̸�����
        showPreview = EditorGUILayout.Toggle("�̸����� ǥ��", showPreview);
        if (showPreview)
        {
            previewColor = EditorGUILayout.ColorField("�̸����� ����", previewColor);
        }

        EditorGUILayout.Space();

        // ��ư��
        GUI.enabled = selectedGenerator != null;

        if (GUILayout.Button("���� ���� ����", GUILayout.Height(30)))
        {
            GenerateVoxelBoss();
        }

        if (GUILayout.Button("���� ���� �����"))
        {
            ClearExistingVoxels();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("���������� ����"))
        {
            SaveAsPrefab();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("1. ���� �����⸦ �����ϼ���\n2. ���� Ÿ���� ����ּ���\n3. '���� ���� ����' ��ư�� ��������", MessageType.Info);
    }

    private void CreateNewVoxelGenerator()
    {
        GameObject newObj = new GameObject("VoxelBossGenerator");
        VoxelGenerator generator = newObj.AddComponent<VoxelGenerator>();

        // �⺻�� ����
        generator.voxelSize = voxelSize;
        generator.bossSize = bossSize;

        selectedGenerator = generator;
        Selection.activeGameObject = newObj;

        Debug.Log("�� ���� �����Ⱑ �����Ǿ����ϴ�!");
    }

    private void GenerateVoxelBoss()
    {
        if (selectedGenerator == null) return;

        // ���� ����
        selectedGenerator.voxelSize = voxelSize;
        selectedGenerator.bossSize = bossSize;
        selectedGenerator.voxelMaterial = voxelMaterial;
        selectedGenerator.glitchMaterial = glitchMaterial;

        // ����
        selectedGenerator.GeneratePixelatedBoss(selectedBossType);

        // Undo ����
        Undo.RegisterCompleteObjectUndo(selectedGenerator.gameObject, "Generate Voxel Boss");

        Debug.Log($"{selectedBossType} ���� ������ �����Ǿ����ϴ�!");
    }

    private void ClearExistingVoxels()
    {
        if (selectedGenerator == null) return;

        Undo.RegisterCompleteObjectUndo(selectedGenerator.gameObject, "Clear Voxels");

        Debug.Log("���� �������� ���ŵǾ����ϴ�.");
    }

    private void SaveAsPrefab()
    {
        if (selectedGenerator == null) return;

        string path = EditorUtility.SaveFilePanelInProject(
            "������ ����",
            $"VoxelBoss_{selectedBossType}",
            "prefab",
            "���� ���� �������� ������ ��ġ�� �����ϼ���");

        if (!string.IsNullOrEmpty(path))
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(selectedGenerator.gameObject, path);
            Debug.Log($"���� ���� �������� ����Ǿ����ϴ�: {path}");

            // ������Ʈ â���� ���̶���Ʈ
            EditorGUIUtility.PingObject(prefab);
        }
    }
}