using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase1State_Ransomware : BossPhaseBase<Ransomware>
{
    public Phase1State_Ransomware(Ransomware owner) : base(owner)
    {
    }

    public override void Enter()
    {
        Debug.Log("�������� ���� ������1 ����");
        owner.GetComponent<Animator>()?.SetTrigger("Phase1");
    }

    public override void Update()
    {
        Debug.Log("[Phase1_Ransomeware] Exit");
    }
}
