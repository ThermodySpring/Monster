using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //�߻� ��ġ;
    public GameObject bulletPrefab;
    public MonsterStatus monsterStatus;

    // HitScan Method
    [SerializeField] LineRenderer lineRenderer;
    RaycastHit hitInfo;
    Vector3 fireDirection; // �߻� ����: ���� ���� ����
    float laserRange = 100f; // ������ ��Ÿ�
    Vector3 playerPos;
    private PlayerStatus player;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) return;

        // �⺻ ����
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.enabled = false;

        player = FindObjectOfType<PlayerStatus>();
        Debug.Log(player.transform.position);
        

    }



    public void Fire()
    {
        playerPos = GameObject.Find("Player").transform.position;
        Quaternion spawnRotation = Quaternion.LookRotation(playerPos - firePoint.position);
        // ������ ȸ������ �Ѿ� ����
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, spawnRotation);
        bullet.GetComponent<MProjectile>().SetBulletDamage(monsterStatus.GetAttackDamage()*monsterStatus.CalculateCriticalHit());
    }

    public void FireLaser()
    {
        // ����ĳ��Ʈ�� ���� ����
        fireDirection = transform.forward; // �߻� ����: ���� ���� ����
        laserRange = 100f; // ������ ��Ÿ�

        // ����ĳ��Ʈ ����
        if (Physics.Raycast(firePoint.position, fireDirection, out hitInfo, laserRange))
        {
            Debug.Log($"Hit: {hitInfo.collider.name}");

            // ��Ʈ ����� �÷��̾����� Ȯ��
            if (hitInfo.collider.CompareTag("Player"))
            {
                // ������ ó��
                PlayerStatus playerStatus = hitInfo.collider.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    // ������ ���ݷ��� ������� �÷��̾�� ������ ����
                    float damage = monsterStatus.GetAttackDamage() * monsterStatus.CalculateCriticalHit();
                    playerStatus.DecreaseHealth(damage);
                }
            }

        }
    }

    public void AimReady()
    {
        // ����ĳ��Ʈ�� ���� ����
        fireDirection = transform.forward; // �߻� ����: ���� ���� ����
        laserRange = 100f; // ������ ��Ÿ�

        DrawLaserEffect(firePoint.position, firePoint.position + fireDirection * laserRange);
    }


    private void DrawLaserEffect(Vector3 start, Vector3 end)
    {
        // LineRenderer�� Ȱ���� ������ �ð� ȿ�� ����
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true; // LineRenderer Ȱ��ȭ
            lineRenderer.SetPosition(0, start); // ������
            lineRenderer.SetPosition(1, end);   // ����
            StartCoroutine(DisableLaser(lineRenderer, 2.1f)); // 0.1�� �� ��Ȱ��ȭ
        }
    }

    // ������ ȿ�� ��Ȱ��ȭ
    private IEnumerator DisableLaser(LineRenderer lineRenderer, float duration)
    {
        yield return new WaitForSeconds(duration);
        lineRenderer.enabled = false;
    }
}

