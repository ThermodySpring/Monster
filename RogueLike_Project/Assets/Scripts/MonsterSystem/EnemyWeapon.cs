using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] Transform firePoint; //�߻� ��ġ;
    public GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire(Quaternion rotation)
    {
        // ������ ȸ������ �Ѿ� ����
        Instantiate(bulletPrefab, firePoint.position, rotation);
    }
}

