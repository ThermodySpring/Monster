using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{   
    public NavMeshSurface navMeshSurface;
    [SerializeField] NavMeshAgent[] agents;

    void Start()
    {
        // ó���� NavMesh�� ����
        UpdateNavMesh();
    }

    public void UpdateNavMesh()
    {
        // ���� NavMesh�� ����� ���Ӱ� ����
        navMeshSurface.BuildNavMesh();

    }

    // Wave�� ���۵ǰų� ���� ��ȭ�� �� �� �޼��带 ȣ��
   
}
