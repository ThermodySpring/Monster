using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("��ų�� ����")]
    [SerializeField] private float skillChangeInterval = 10f;  // ��ų ���� ����
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    [SerializeField] private int maxLockedSkills = 2;         // ���ÿ� ��� �� �ִ� �ִ� ��ų ��
    [SerializeField] private float duration = 5f;

    [Header("�ð� ȿ�� ����")]
    [SerializeField] private float glitchEffectDuration = 1.0f;   // �۸�ġ ȿ�� ���� �ð�
    [SerializeField] private float glitchEffectInterval = 15.0f;  // �۸�ġ ȿ�� ����

    // ���� ����
    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private bool isActive = false;

    // ����
    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;

    // ��� �� �ִ� ��ų ���
    private List<SkillType> lockableSkills = new List<SkillType>();

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
    }

    public override void Enter()
    {
        Debug.Log("Phase2_RansomLock_State: ���� ����");

        // �ʿ��� ������Ʈ ���� ã��
        InitializeReferences();

        // ��� �� �ִ� ��ų ��� �ʱ�ȭ
        InitializeLockableSkills();

        // ���� �ʱ�ȭ
        nextGlitchTime = glitchEffectInterval;
        nextSkillChangeTime = skillChangeInterval;

        // UI �ʱ�ȭ
        SetupUI();

        // ù ��ų�� ����
        ApplyLockPattern();

        // �۸�ġ ȿ�� ���
        PlayGlitchEffect();

        // ���� �˸�
        Debug.Log("�������� ��ų�� ����: ���� " + patternType);
    }

    public override void Exit()
    {
        Debug.Log("Phase2_RansomLock_State: ���� ����");

        // UI ����
        CleanupUI();

        isActive = false;
    }

    public override void Update()
    {
        if (!isActive) return;

        // Ÿ�̸� ������Ʈ

        // ��ų ���� �ð��� �Ǹ� ���� ����
        ApplyLockPattern();

        // �۸�ġ ȿ�� ���
        PlayGlitchEffect();

        // �ֱ����� �۸�ġ ȿ�� ������Ʈ
        UpdateGlitchEffects();

        CompleteState();
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    private void InitializeReferences()
    {
        // UI �Ŵ��� ã��
        uiManager = GameObject.FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager�� ã�� �� �����ϴ�. UI ���� ����� �۵����� ���� �� �ֽ��ϴ�.");
        }

        // �÷��̾� ��Ʈ�� ã��
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        if (playerControl == null)
        {
            Debug.LogError("PlayerControl�� ã�� �� �����ϴ�!");
        }

        // �÷��̾� ���� ã��
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerStatus�� ã�� �� �����ϴ�!");
        }

        // ����Ʈ ���μ��� �Ŵ��� ã��
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
        if (postProcessingManager == null)
        {
            Debug.LogWarning("PostProcessingManager�� ã�� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ��� �� �ִ� ��ų ��� �ʱ�ȭ
    /// </summary>
    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();

        // ��������� ��� �� �ִ� �ٽ� ��ų��
        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);

        // �ʿ��ϴٸ� ���� ���� ��ų�� �߰� ����
        // lockableSkills.Add(SkillType.Shooting);
    }

    /// <summary>
    /// UI ����
    /// </summary>
    private void SetupUI()
    {
        if (uiManager == null) return;

        // UI�� �������� ��� ǥ��
        // uiManager.ShowRansomLockUI(stateDuration);
        // uiManager.ShowNotification("�ý����� �����Ǿ����ϴ�! ������� ���� ����� ���ѵ˴ϴ�.");

        // ȭ�� ȿ�� ����
        if (postProcessingManager != null)
        {
            // ��: �������� ���� ȿ�� (����� ���� ����, ������ Ȱ��ȭ ��)
            // postProcessingManager.ChangeVignetteColor(Color.red);
            // postProcessingManager.ChangeChromaticAberrationActive(true);
        }
    }

    /// <summary>
    /// UI ����
    /// </summary>
    private void CleanupUI()
    {
        if (uiManager == null) return;

        // UI���� �������� ��� ǥ�� ����
        // uiManager.HideRansomLockUI();

        // ȭ�� ȿ�� ����
        if (postProcessingManager != null)
        {
            // ��: ȭ�� ȿ�� ����
            // postProcessingManager.ChangeVignetteColor(Color.white);
            // postProcessingManager.ChangeChromaticAberrationActive(false);
        }
    }

    /// <summary>
    /// ������ ���Ͽ� ���� ��ų ��� ����
    /// </summary>
    private void ApplyLockPattern()
    {
        if (playerControl == null || lockableSkills.Count == 0) return;

        // ���Ͽ� ���� �ٸ� ��� ����
        switch (patternType)
        {
            case RandomSkillLockController.LockPatternType.Fixed:
                ApplyFixedPattern();
                break;

            case RandomSkillLockController.LockPatternType.RandomRotation:
                ApplyRandomRotationPattern();
                break;

            case RandomSkillLockController.LockPatternType.PulsatingLock:
                ApplyPulsatingPattern();
                break;

            case RandomSkillLockController.LockPatternType.ProgressivelyWorse:
                ApplyProgressivePattern();
                break;

            case RandomSkillLockController.LockPatternType.CompletelyRandom:
                ApplyRandomPattern();
                break;
        }
    }

    /// <summary>
    /// ���� ���� ���� - �׻� ���� ��ų ���
    /// </summary>
    private void ApplyFixedPattern()
    {
        // ù ��° �� ���� ��ų�� ��� (������ ���� ���� ����)
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            owner.LockPlayerSkill(lockableSkills[i], duration);
        }
    }

    /// <summary>
    /// ���� ��ȯ ���� - �Ź� �ٸ� ��ų�� ��ȯ�ϸ� ���
    /// </summary>
    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        // ��ȯ ��ġ ��� (��ȯ �ֱ�� lockableSkills ����)
        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;

        // �ش� �ε����� ��ų ���
        owner.LockPlayerSkill(lockableSkills[cycleIndex], duration);
    }

    /// <summary>
    /// �ƹ� ���� - ��� ��ų ��� �� ������ �ݺ�
    /// </summary>
    private void ApplyPulsatingPattern()
    {
        // ¦�� �ֱ⿡�� ���, Ȧ�� �ֱ⿡�� �ƹ��͵� ����� ����
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                owner.LockPlayerSkill(lockableSkills[i], duration);
            }
        }
        // shouldLock�� false�� ��� ��ų�� ���� ���·� ����
    }

    /// <summary>
    /// ������ ��ȭ ���� - �ð��� �������� �� ���� ��ų ���
    /// </summary>
    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        // ���� ���൵�� ���� ��� ��ų �� ����
        int skillsToLock = 0;
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        // ���纻 ���� �� ����
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // ���� ����ŭ ��ų ���
        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    /// <summary>
    /// ���� ���� ���� - �Ź� ������ ������ ��ų ���
    /// </summary>
    private void ApplyRandomPattern()
    {
        if (lockableSkills.Count == 0) return;

        // ���纻 ���� �� ����
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // �������� ��� ��ų �� ����
        int skillsToLock = Random.Range(1, Mathf.Min(maxLockedSkills, shuffledSkills.Count) + 1);

        // ���õ� ����ŭ ��ų ���
        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    /// <summary>
    /// ���� �Ϸ� �� ���� ���·� ��ȯ
    /// </summary>
    private void CompleteState()
    {
        Debug.Log("�������� ��� ���� �Ϸ�");

        // ���� ���·� ��ȯ
        // owner.ChangeState(owner.GetNextState(this));
    }

    /// <summary>
    /// �۸�ġ ȿ�� ������Ʈ
    /// </summary>
    private void UpdateGlitchEffects()
    {
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }
    }

    /// <summary>
    /// �۸�ġ ȿ�� ���
    /// </summary>
    private void PlayGlitchEffect()
    {
        Debug.Log("�۸�ġ ȿ�� ���");

        // UI �۸�ġ ȿ��
        if (uiManager != null)
        {
            // uiManager.PlayGlitchEffect(glitchEffectDuration);
        }

        // ȭ�� ȿ�� (����Ʈ ���μ���)
        if (postProcessingManager != null)
        {
            // ��: ȭ�� ������ ȿ��
            // postProcessingManager.FlashEffect(Color.cyan, 0.2f);
        }
    }

    /// <summary>
    /// ����Ʈ ���� (Fisher-Yates �˰���)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
}