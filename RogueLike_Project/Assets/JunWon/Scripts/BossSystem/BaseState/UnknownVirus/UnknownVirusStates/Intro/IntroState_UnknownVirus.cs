using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroState_UnknownVirus : BaseState_UnknownVirus
{
    private float timer = 0f;
    private const float introDuration = 3f;

    public IntroState_UnknownVirus(UnknownVirusBoss owner) : base(owner) { }

    public override void Enter()
    {
        Debug.Log("UnknownVirus: Intro State ����");
        timer = 0f;

        // ���� �ʱ� ���� ����
        owner.BossStatus.SetMovementSpeed(5f);
        owner.BossStatus.SetAttackDamage(20f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        // ��Ʈ�� �ð��� ������ �ڵ����� ���� ���·� ��ȯ (Ʈ������)
    }

    public override void Exit()
    {
        Debug.Log("UnknownVirus: Intro State ����");
    }
}
