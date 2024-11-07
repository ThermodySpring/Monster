using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSphere : MonoBehaviour
{
    // Start is called before the first frame update
    WaveManager waveManager;
    bool isActived;
    void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
        isActived = false;
    }
    private void OnEnable()
    {
        isActived = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //�÷��̾ �������� �� ��ư�Է��� ����ϴ� �ڷ�ƾ ����
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine("PressToSwitchWave");
        }

    }

    
    private void OnTriggerExit(Collider other)
    {
        //�÷��̾ �������� ����� �� �Է°��� �ߴ�
        if (other.gameObject.tag == "Player")
        {
            StopCoroutine("PressToSwitchWave");
        }
    }
    
    IEnumerator PressToSwitchWave()
    {
        while (!isActived)
        {
            if(Input.GetKey(KeyCode.F))
            {
                // F �Է����� �� ��ȣ �۽�
             //   Debug.Log("Switching!");
                waveManager.NextWaveTrigger = true;
                isActived=true;
            }
        //    Debug.Log("Press to Switch Wave!");

            yield return null;
        }
    }
}
