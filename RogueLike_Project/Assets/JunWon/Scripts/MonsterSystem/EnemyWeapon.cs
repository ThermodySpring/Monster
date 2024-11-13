using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //�߻� ��ġ;
    public GameObject bulletPrefab;
    public SkinnedMeshRenderer smRenderer;
    public MonsterStatus monsterStatus;

    // Start is called before the first frame update
    void Start()
    {
        firePoint = smRenderer.GetComponent<SkinnedMeshRenderer>().bones[0].transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire()
    {

        // ������ ȸ������ �Ѿ� ����
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
        bullet.GetComponent<MProjectile>().SetBulletDamage(monsterStatus.GetAttackDamage()*monsterStatus.CalculateCriticalHit());
    }
}

