using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState_UnknownVirus : BaseState_UnknownVirus
{
    public TransformState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Combat State ����");
    }

    public override void Update()
    {
        // ���� ���¿����� ���� Ȱ��ȭ�� ���� ���°� ������ ���
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Combat State ����");
    }
}