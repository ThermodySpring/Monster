using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    public float maxRadius = 10f; // ����� �ִ� �ݰ�
    public float expansionSpeed = 5f; // ����� Ȯ�� �ӵ�
    public float damage = 20f; // ������
    public LayerMask targetLayer; // ������ ���� ���̾�
    public float duration = 2f; // ����� ���� �ð�

    private HashSet<Collider> hitTargets = new HashSet<Collider>(); // �������� ���� ���
    private float innerRadius = 0f; // ����� ���� �ݰ�
    private float outerRadius = 0f; // ����� �ܺ� �ݰ�
    private Transform shockwaveVisual;

    private void Start()
    {
        damage = GetComponentInParent<MonsterStatus>().GetAttackDamage();
        shockwaveVisual = transform.GetChild(0); // �ڽ� ������Ʈ�� �ð��� ȿ�� ����
        shockwaveVisual.localScale = Vector3.zero; // �ʱ� ũ��
    }

    private void Update()
    {
        // ����� Ȯ��
        innerRadius = outerRadius; // ���� �ܺ� �ݰ��� ���� �ݰ����� ������Ʈ
        outerRadius = Mathf.Min(outerRadius + expansionSpeed * Time.deltaTime, maxRadius);

        shockwaveVisual.localScale = Vector3.one * outerRadius * 2f; // �ð��� ȿ�� ������Ʈ

        // ����� ��ο� ���� ���Ե� ��� ó��
        ApplyShockwaveEffect();

        // �ִ� �ݰ濡 �����ϸ� ����
        if (outerRadius >= maxRadius)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyShockwaveEffect()
    {
        // ���� �ݰ� ���� �ȿ� �ִ� ��� �浹ü �˻�
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, outerRadius, targetLayer);

        foreach (var collider in hitColliders)
        {
            // ������� "���� ���Ե�" ������ �ִ� ��� ó��
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < innerRadius || hitTargets.Contains(collider)) continue;

            // ��� ������ ó��
            PlayerStatus player = collider.GetComponent<PlayerStatus>();
            if (player != null)
            {
                player.DecreaseHealth(damage);
            }

            // ������ ���� ��Ͽ� �߰�
            hitTargets.Add(collider);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ����׿� ����� ���� �� �ܺ� �ݰ� �ð�ȭ
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, outerRadius);
    }
}
