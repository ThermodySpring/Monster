using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class DeathFragmentSystem : MonoBehaviour
{

    [Header("���� ����")]
    [SerializeField] private List<Rigidbody> voxelChildren = new List<Rigidbody>();
    [SerializeField] private Transform[] fragmentPrefabs; // �̸� ���� ���� �����յ�
    [SerializeField] private bool useProceduralFragments = true; // �ڵ����� ���� ��������
    [SerializeField] private int fragmentCount = 15; // ������ ���� ����
    [SerializeField] private Vector3 fragmentSize = new Vector3(0.5f, 0.5f, 0.5f); // ���� ũ��

    [Header("���� ȿ��")]
    [SerializeField] private float explosionForce = 300f; // ���� ��
    [SerializeField] private float explosionRadius = 5f; // ���� �ݰ�
    [SerializeField] private Vector3 explosionUpward = Vector3.up; // ���� ���� ��
    [SerializeField] private float fragmentLifetime = 10f; // ������ ������� �ð�

    [Header("�߷� �� ����")]
    [SerializeField] private float gravityMultiplier = 1f; // �߷� ���
    [SerializeField] private float bounceForce = 0.3f; // �ٿ ��
    [SerializeField] private PhysicMaterial fragmentPhysicMaterial; // ���� ����

    [Header("�ð� ȿ��")]
    [SerializeField] private Material fragmentMaterial; // ���� ����
    [SerializeField] private GameObject explosionEffect; // ���� ����Ʈ
    [SerializeField] private AudioClip explosionSound; // ���� ����
    [SerializeField] private bool fadeOutFragments = true; // ���� ���̵�ƿ�
    [SerializeField] private float fadeStartTime = 5f; // ���̵� ���� �ð�

    [Header("��ƼŬ ȿ��")]
    [SerializeField] private ParticleSystem dustParticles; // ���� ��ƼŬ
    [SerializeField] private ParticleSystem sparkParticles; // ����ũ ��ƼŬ

    private AudioSource audioSource;
    private List<GameObject> createdFragments = new List<GameObject>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                voxelChildren.Add(rb);
            }
        }
    }

    /// <summary>
    /// ���� ��� �� ���� ����߸��� ����
    /// </summary>
    public void TriggerDeathFragmentation()
    {
        StartCoroutine(DeathFragmentationSequence());
    }

    /// <summary>
    /// ��� ���� ������
    /// </summary>
    private IEnumerator DeathFragmentationSequence()
    {
        // 1. ���� ����Ʈ �� ����
        PlayExplosionEffects();

        // 2. ��� ��� (���尨 ����)
        yield return new WaitForSeconds(0.3f);

        // 3. ������ ����Ʈ���� 
        StartCoroutine(FallFragments());

        // 5. ������ ���̵�ƿ� ����
        //if (fadeOutFragments)
        //{
        //    yield return new WaitForSeconds(fadeStartTime);
        //    StartCoroutine(FadeOutFragments());
        //}

        // 6. ���� �ð� �� ��� ���� ����
        yield return new WaitForSeconds(fragmentLifetime);
        CleanupAllFragments();
    }


    

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    private void SetupFragmentPhysics(GameObject fragment)
    {
        // Rigidbody �߰� �� ����
        Rigidbody rb = fragment.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fragment.AddComponent<Rigidbody>();
        }

        // �߷� ����
        rb.useGravity = true;
        rb.mass = Random.Range(0.5f, 2f);
        rb.drag = Random.Range(0.1f, 0.3f);
        rb.angularDrag = Random.Range(0.1f, 0.5f);

        // ���� ���� ����
        Collider col = fragment.GetComponent<Collider>();
        if (col != null && fragmentPhysicMaterial != null)
        {
            col.material = fragmentPhysicMaterial;
        }

        // ���� �� ����
        Vector3 explosionDirection = (fragment.transform.position - transform.position).normalized;
        Vector3 force = explosionDirection * explosionForce + explosionUpward * explosionForce * 0.5f;

        // ���� ��� �߰�
        force += Random.insideUnitSphere * explosionForce * 0.3f;

        rb.AddForce(force);
        rb.AddTorque(Random.insideUnitSphere * explosionForce * 0.5f);

        // ������ ������Ʈ �߰�
        fragment.AddComponent<FragmentBehavior>().Initialize(fragmentLifetime, fadeStartTime, fadeOutFragments);
    }

    /// <summary>
    /// ���� ����Ʈ ���
    /// </summary>
    private void PlayExplosionEffects()
    {
        // ���� ����Ʈ
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 5f);
        }

        // ���� ����
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // ��ƼŬ ȿ��
        if (dustParticles != null)
        {
            dustParticles.Play();
        }

        if (sparkParticles != null)
        {
            sparkParticles.Play();
        }
    }

    private IEnumerator FallFragments()
    {
        foreach (Rigidbody fragment in voxelChildren)
        {
            fragment.useGravity = true;
            fragment.isKinematic = false;
        }

         yield return null;
    }

    /// <summary>
    /// ������ ���̵�ƿ�
    /// </summary>
    private IEnumerator FadeOutFragments()
    {
        float fadeDuration = fragmentLifetime - fadeStartTime;

        foreach (Rigidbody fragment in voxelChildren)
        {
            if (fragment != null)
            {
                FragmentBehavior fragmentBehavior = fragment.GetComponent<FragmentBehavior>();
                if (fragmentBehavior != null)
                {
                    fragmentBehavior.StartFadeOut(fadeDuration);
                }
            }
        }

        yield return null;
    }

    /// <summary>
    /// ��� ���� ����
    /// </summary>
    private void CleanupAllFragments()
    {
        foreach (GameObject fragment in createdFragments)
        {
            if (fragment != null)
            {
                Destroy(fragment);
            }
        }

        createdFragments.Clear();

        // ���� ������Ʈ�� ���� (�ʿ��� ���)
        Destroy(gameObject, 1f);
    }
}
