using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnknownVirusBoss;

public class BossCombatContext
{
    // �Ÿ� ����
    public float DistanceToPlayer { get; set; }
    public bool IsPlayerInMeleeRange { get; set; }
    public bool IsPlayerInRangedRange { get; set; }

    // �÷��̾� ����
    public float PlayerHealthRatio { get; set; }
    public bool IsPlayerStunned { get; set; }
    public bool IsPlayerUsingRangedWeapon { get; set; }

    // ���� ����
    public float BossHealthRatio { get; set; }
    public float TimeSinceLastFormChange { get; set; }
    public BossForm CurrentForm { get; set; }

    // ȯ�� ���
    public bool IsNearWall { get; set; }
    public bool IsOnElevatedGround { get; set; }
    public float ArenaRemaining { get; set; } // ���� ���� �ܿ� ����

    // �ɷ� ��ٿ�
    public Dictionary<string, float> AbilityCooldowns { get; set; }

    public BossCombatContext()
    {
        AbilityCooldowns = new Dictionary<string, float>();
    }
}
