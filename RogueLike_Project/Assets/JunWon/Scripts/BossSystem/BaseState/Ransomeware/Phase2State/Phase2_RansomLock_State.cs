using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("��ų ��� ����")]
    [SerializeField] private float skillChangeInterval = 10f;  // ��ų ���� ����
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    [SerializeField] private int maxLockedSkills = 2;         // ���ÿ� ��� �� �ִ� �ִ� ��ų ��
    [SerializeField] private float lockDuration = 15f;        // ��ų ��� ���� �ð�
    [SerializeField] private float stateMaxDuration = 30f;    // ��ü ��ų ��� ���� ���� �ð�

    [Header("�۸�ġ ȿ�� ����")]
    [SerializeField] private float glitchEffectDuration = 1.0f;   // �۸�ġ ȿ�� ���� �ð�
    [SerializeField] private float glitchEffectInterval = 15.0f;  // �۸�ġ ȿ�� ����

    // ���� ����
    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private float stateTimeRemaining = 0f;
    private bool isActive = false;
    private bool isAttackFinished = false;

    // ������Ʈ ����
    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;
    private SkillLockUI skillLockUI;
    public static event RandomSkillLockController.SkillLockEvent OnRansomLockEvent;


    // ��ų ���� ����Ʈ
    private List<SkillType> lockableSkills = new List<SkillType>();
    private List<SkillType> currentlyLockedSkills = new List<SkillType>();

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
        owner.SetLockState(this);
    }


    public override void Enter()
    {
        isAttackFinished = false;
        Debug.Log("[Phase2_RansomLock_State] Enter");
        owner.NmAgent.isStopped = true;

        // �ʿ��� ������Ʈ ���� ã��
        InitializeReferences();

        // ��� �� �ִ� ��ų ��� �ʱ�ȭ
        InitializeLockableSkills();

        // ���� �ʱ�ȭ
        stateTimeRemaining = stateMaxDuration;
        nextGlitchTime = Time.time + glitchEffectInterval * 0.5f;
        nextSkillChangeTime = Time.time + skillChangeInterval;
        isActive = true;

        // UI �ʱ�ȭ
        SetupUI();

        if (CanExecuteAttack())
        {
            // �ִϸ��̼� Ʈ���� ����
            owner.Animator.SetTrigger("RansomLock");
            if (owner.AbilityManager.UseAbility("Lock"))
            {
                // �ִϸ��̼��� �˾Ƽ� �����
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("[Phase2_RansomLock_State] Exit");

        // UI ����
        CleanupUI();

        // ��� �÷��̾� ��ų ��� ����
        if (playerControl != null)
        {
            playerControl.UnlockAllSkills();
        }

        isActive = false;
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RansomLock");
    }

    public override void Update()
    {
        if (!isActive || isInterrupted) return;

        // ��ų ��� ���� ��ü ���� �ð� üũ
        stateTimeRemaining -= Time.deltaTime;
        if (stateTimeRemaining <= 0)
        {
            isAttackFinished = true;
            return;
        }

        // �������� �۸�ġ ȿ�� üũ
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }

        // �������� ��ų ���� üũ
        if (Time.time >= nextSkillChangeTime)
        {
            ApplyLockPattern();
            nextSkillChangeTime = Time.time + skillChangeInterval;
        }
    }

    private bool CanExecuteAttack()
    {
        return owner.Player != null;
    }

    public void OnAttackFinished()
    {
        isAttackFinished = true;
    }

    public bool IsAnimationFinished() => isAttackFinished;

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnStartLockEffect()
    {
        Debug.Log("[Phase2_RansomLock_State] OnStartLockEffect");

        // ù ��ų�� ����
        ApplyLockPattern();

        // �۸�ġ ȿ�� ���
        PlayGlitchEffect();
    }

    private void InitializeReferences()
    {
        uiManager = GameObject.FindObjectOfType<UIManager>();
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
        skillLockUI = GameObject.FindObjectOfType<SkillLockUI>();
    }

    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();
        currentlyLockedSkills.Clear();

        // ��� �� �ִ� ��ų ��� ����
        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);

        // ���� �÷��̿� ū ������ ���� �ʵ��� ���� ��ų���� ����
        // lockableSkills.Add(SkillType.Shooting);
        // lockableSkills.Add(SkillType.WeaponSwitch);
        // lockableSkills.Add(SkillType.Interaction);
    }

    private void SetupUI()
    {
        if (uiManager == null) return;

        // �ʿ��� UI ������Ʈ Ȱ��ȭ
        if (postProcessingManager != null)
        {
            postProcessingManager.EnableGlitchEffect(0.2f);
        }
    }

    private void CleanupUI()
    {
        // �۸�ġ ȿ�� ��Ȱ��ȭ
        if (postProcessingManager != null)
        {
            postProcessingManager.DisableGlitchEffect();
        }

        // ��ų ��� UI �����
        if (skillLockUI != null)
        {
            TriggerLockEvent("LockEnded", new List<SkillType>(), 0);
        }
    }

    private void ApplyLockPattern()
    {
        if (playerControl == null || lockableSkills.Count == 0) return;

        // ������ ��� ��� ��ų ����
        UnlockAllSkills();
        currentlyLockedSkills.Clear();

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

        // ���� ��� ��ų �α� ���
        LogLockedSkills();

        // UI ������Ʈ�� ���� �̺�Ʈ �߻�
        TriggerLockEvent("SkillsChanged", currentlyLockedSkills, lockDuration);
    }

    private void ApplyFixedPattern()
    {
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            LockSkill(lockableSkills[i]);
        }
    }

    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;
        LockSkill(lockableSkills[cycleIndex]);

        // �� ��° ��ų�� �����ϰ� ���� (ù ��°�� �ٸ� ��ų)
        if (lockableSkills.Count > 1 && Random.value > 0.5f)
        {
            int secondIndex = (cycleIndex + 1 + Random.Range(0, lockableSkills.Count - 1)) % lockableSkills.Count;
            LockSkill(lockableSkills[secondIndex]);
        }
    }

    private void ApplyPulsatingPattern()
    {
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                LockSkill(lockableSkills[i]);
            }
        }
    }

    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        // ������Ʈ ���൵�� ���� ��� ��ų �� ���� (�ð��� ������ �� ���� ��ų ���)
        float progress = 1.0f - (stateTimeRemaining / stateMaxDuration);
        int skillsToLock = Mathf.CeilToInt(progress * maxLockedSkills);
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        // ���纻 ���� �� ����
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // ���� ����ŭ ��ų ���
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
        }
    }

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
            LockSkill(shuffledSkills[i]);
        }
    }

    private void LockSkill(SkillType skillType)
    {
        if (playerControl == null) return;

        // �÷��̾� ��ų ��Ȱ��ȭ
        playerControl.SetSkillEnabled(skillType, false);
        currentlyLockedSkills.Add(skillType);

        // ���� �������� ���������� ���� �ð����� ��ų�� ���
        owner.LockPlayerSkill(skillType, lockDuration);
    }

    private void UnlockAllSkills()
    {
        if (playerControl == null) return;

        foreach (SkillType skill in currentlyLockedSkills)
        {
            playerControl.SetSkillEnabled(skill, true);
        }
    }

    private void LogLockedSkills()
    {
        if (currentlyLockedSkills.Count == 0)
        {
            Debug.Log("���� ��� ��ų ����");
            return;
        }

        string skills = "";
        foreach (SkillType skill in currentlyLockedSkills)
        {
            skills += skill.ToString() + ", ";
        }

        if (skills.Length > 2)
        {
            skills = skills.Substring(0, skills.Length - 2);
        }

        Debug.Log($"���� ��� ��ų: {skills}");
    }

    private void PlayGlitchEffect()
    {
        // ����Ʈ ���μ��� �۸�ġ ȿ�� ���
        if (postProcessingManager != null)
        {
            postProcessingManager.TriggerGlitchEffect(glitchEffectDuration);
        }

        // UI �۸�ġ ȿ�� ���
        if (skillLockUI != null)
        {
            skillLockUI.PlayGlitchEffect();
        }
    }

    private void TriggerLockEvent(string eventType, List<SkillType> skills, float remainingTime)
    {
        // RandomSkillLockController���� ���ǵ� �̺�Ʈ ������ ���� ȣ��
        OnRansomLockEvent?.Invoke(eventType, new List<SkillType>(skills), remainingTime);
    }

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