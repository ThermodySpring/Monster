using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Prototype : MonoBehaviour
{
    [SerializeField] GameObject tile;
    [SerializeField] GameObject laser;
    [SerializeField] TMP_Text text;
    GameObject[] tiles;
    
    // Ÿ���� 2*4*2 ũ��� 4*4���� ��ġ
    void Start()
    {
        tiles = new GameObject[8 * 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                tiles[i * 8 + j] = Instantiate(tile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);
            }
        }

        StartCoroutine(StartSearch());
    }
    //SearchTile, SearchTileBinary, SearchTileRandom �ڷ�ƾ�� �������� �����ϴ� �� �ݺ��ϴ� �ڷ�ƾ
    IEnumerator StartSearch()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, tiles.Length);
            int randomMethod = Random.Range(0, 3);
            switch (randomMethod)
            {
                case 0:
                    yield return StartCoroutine(SearchTile(randomIndex));
                    break;
                case 1:
                    yield return StartCoroutine(SearchTileBinary(randomIndex));
                    break;
                case 2:
                    yield return StartCoroutine(SearchTileRandom(randomIndex));
                    break;
            }
        }
    }


    //tiles �迭�� ������Ű�� ��� Ÿ���� 0.5�ʵ��� ȸ������ �ٲ۵� ������� �����ϴ� �ڷ�ƾ
    IEnumerator ReverseTiles()
    {
        text.text = "Reverse Tiles";
        // tiles �迭�� ����
        for (int i = 0; i < tiles.Length / 2; i++)
        {
            GameObject temp = tiles[i];
            tiles[i] = tiles[tiles.Length - 1 - i];
            tiles[tiles.Length - 1 - i] = temp;
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.gray);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
        }
        yield return new WaitForSeconds(0.5f);
    }

    //Ÿ���� ���� ��ġ�� ���� Ž���ϴ� ������ �ð������� �����ִ� ������ �ڷ�ƾ���� �ۼ�
    IEnumerator SearchTile(int index)
    {
        //50% Ȯ���� tiles �迭�� ����
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Linear Search";

        for (int i = 0; i < tiles.Length; i++)
        {
            if (i == index)
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.1f);
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            else
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                Destroy(Instantiate(laser, tiles[i].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                yield return new WaitForSeconds(0.1f);
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }

        }
        yield return new WaitForSeconds(1f);
        //StartCoroutine(SearchTileBinary(Random.Range(1, tiles.Length)));
    }
    //�̹��� �̺� Ž������ Ÿ���� ã�� ������ �ڷ�ƾ���� �ۼ�. Ÿ���� ������ �� ��ǥ Ÿ���� ���Ե� �κ� �迭�� ��� Ÿ���� ���������� �ٲٰ� �������� ������� �ٲ㼭 �ð������� ������. �� �������� �������� 0.5�ʸ� ǥ���ϰ� ������� ����
    IEnumerator SearchTileBinary(int index)
    {
        //50% Ȯ���� tiles �迭�� ����
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Binary Search";
        int left = 0;
        int right = tiles.Length - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;

            // ����: ���� Ž�� ������ ������
            if(right - left < tiles.Length - 1)
            {
                for (int i = left; i <= right; i++)
                {
                    tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                    Destroy(Instantiate(laser, tiles[i].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                }
            }
            

            // 0.5�� ���� ���� ����
            yield return new WaitForSeconds(0.5f);

            // ���� ����: �����ߴ� Ÿ�ϸ� �������
            for (int i = left; i <= right; i++)
            {
                tiles[i].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }
            yield return new WaitForSeconds(0.5f);
            // �̺� Ž�� ����
            if (mid == index)
            {
                tiles[mid].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.3f);
                tiles[mid].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            else if (mid < index)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }
        yield return new WaitForSeconds(1f);
        //StartCoroutine(SearchTile(Random.Range(1, tiles.Length)));
    }
    //20ȸ ���� Ž���ϴ� �� �ð�ȭ�ϴ� �ڷ�ƾ
    IEnumerator SearchTileRandom(int index)
    {
        //50% Ȯ���� tiles �迭�� ����
        if (Random.Range(0, 2) == 0)
        {
            yield return StartCoroutine(ReverseTiles());
        }
        text.text = "Random Search";
        for (int i = 0; i < 20; i++)
        {
            int randomIndex = Random.Range(0, tiles.Length);
            //�ش� Ÿ���� ��ǥ Ÿ���̸� �ʷϻ����� �ٲٰ� ���� ����
            if (randomIndex == index)
            {
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
                yield return new WaitForSeconds(0.3f);
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
                break;
            }
            //�ƴϸ� ���������� �ٲٱ�
            else
            {
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
                Destroy(Instantiate(laser, tiles[randomIndex].transform.position + new Vector3(0, 1, 0), Quaternion.identity), 2f);
                yield return new WaitForSeconds(0.1f);
                tiles[randomIndex].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.white);
            }


          
        }
    }





}
