using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridPatternMaker : EditorWindow
{
    private int textureSize = 512;
    private int blockSize = 4;  // DXT5 ��Ÿ���� ��� ũ��
    private float noiseScale = 20f;
    private float contrast = 1.5f;
    private float threshold = 0.5f;
    private bool useCompression = true;

    [MenuItem("Tools/Digital Noise Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridPatternMaker>("Digital Noise");
    }

    private void OnGUI()
    {
        GUILayout.Label("Digital Noise Settings", EditorStyles.boldLabel);

        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        noiseScale = EditorGUILayout.FloatField("Noise Scale", noiseScale);
        contrast = EditorGUILayout.FloatField("Contrast", contrast);
        threshold = EditorGUILayout.Slider("Threshold", threshold, 0f, 1f);
        useCompression = EditorGUILayout.Toggle("Use DXT5 Compression", useCompression);

        if (GUILayout.Button("Generate Noise Texture"))
        {
            GenerateNoiseTexture();
        }
    }

    private float Random2D(Vector2 st)
    {
        // Frac�� �Ҽ��� �κи� ��ȯ�ϴ� �Լ��Դϴ�.
        // x - floor(x)�� ������ ����� ���� �� �ֽ��ϴ�.
        float value = Mathf.Sin(Vector2.Dot(st, new Vector2(12.9898f, 78.233f))) * 43758.5453123f;
        return value - Mathf.Floor(value);
    }

    private void GenerateNoiseTexture()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        // ��� ������ ������ ����
        for (int y = 0; y < textureSize; y += blockSize)
        {
            for (int x = 0; x < textureSize; x += blockSize)
            {
                // ����� �⺻ ������ �� ����
                float blockNoiseBase = Random2D(new Vector2(
                    x / (float)textureSize * noiseScale,
                    y / (float)textureSize * noiseScale
                ));

                // ��� ��� ����
                blockNoiseBase = Mathf.Pow(blockNoiseBase, contrast);

                // �Ӱ谪 ����
                float blockValue = blockNoiseBase > threshold ? 1 : 0;

                // ��� ���� ��� �ȼ��� ����
                for (int by = 0; by < blockSize && (y + by) < textureSize; by++)
                {
                    for (int bx = 0; bx < blockSize && (x + bx) < textureSize; bx++)
                    {
                        // ��� �� ���� �ȼ��� �ణ�� ��ȭ �߰�
                        float pixelNoise = Random2D(new Vector2(bx, by)) * 0.1f;
                        float finalValue = Mathf.Clamp01(blockValue + pixelNoise);

                        pixels[(y + by) * textureSize + (x + bx)] = new Color(finalValue, finalValue, finalValue, 1);
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // ����
        string path = EditorUtility.SaveFilePanel(
            "Save Noise Texture",
            "Assets",
            "DigitalNoise.png",
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // �ؽ�ó ����Ʈ ����
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Point; // �ȼ�ȭ�� ������ ����

                if (useCompression)
                {
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    importer.compressionQuality = 50; // ���� ǰ�� ����
                }
                else
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                }

                importer.SaveAndReimport();
            }
        }
    }
}
