using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSphere : MonoBehaviour
{
    // Start is called before the first frame update
    WaveManager waveManager;
    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //�÷��̾ �������� �� ��ư�Է��� ����ϴ� �ڷ�ƾ ����
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Can Switch Wave");
            StartCoroutine("PressToSwitchWave");
        }

    }

    
    private void OnTriggerExit(Collider other)
    {
        //�÷��̾ �������� ����� �� �Է°��� �ߴ�
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Cannot Switch Wave");
            StopCoroutine("PressToSwitchWave");
        }
    }
    
    IEnumerator PressToSwitchWave()
    {
        while (true)
        {
            if(Input.GetKey(KeyCode.F))
            {
                // F �Է����� �� ��ȣ �۽�
                Debug.Log("Switching!");
                waveManager.IsGameStarted = true;
                Destroy(gameObject);
            }
            Debug.Log("Press to Switch Wave!");

            yield return null;
        }
    }
}
