using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State<Ransomware>
{
    public IdleState(Ransomware owner) : base(owner) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        // �÷��̾ �����ϸ� ChaseState�� ��ȯ
        if (true)
        {
        }
    }

    public override void Exit()
    {
        Debug.Log("Idle State Exit");
    }
}
