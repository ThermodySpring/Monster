using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phase2State_Ransomeware : State<Ransomware>
{
    public Phase2State_Ransomeware(Ransomware owner) : base(owner)
    { }

    private StateMachine<Ransomware> subFsm;

    public override void Enter()
    {
        Debug.Log("�������� ���� ������2 ����");
    }

    public override void Update()
    {
        subFsm.Update();
    }
}
