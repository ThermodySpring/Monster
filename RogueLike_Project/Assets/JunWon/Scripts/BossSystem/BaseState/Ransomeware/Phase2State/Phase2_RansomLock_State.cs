using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;

public class Phase2_RansomLock_State : State<Ransomware>
{
    private float skillChangeInterval = 10f;  // ��ų ���� ����
    private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;
    private int maxLockedSkills = 2;         // ���ÿ� ��� �� �ִ� �ִ� ��ų ��
    private float duration = 5f;

    private float glitchEffectDuration = 1.0f;   // �۸�ġ ȿ�� ���� �ð�
    private float glitchEffectInterval = 15.0f;  // �۸�ġ ȿ�� ����

    private float nextGlitchTime = 0f;
    private float nextSkillChangeTime = 0f;
    private bool isActive = false;
    private bool isAttackFinished = false;

    private UIManager uiManager;
    private PlayerControl playerControl;
    private PlayerStatus playerStatus;
    private PostProcessingManager postProcessingManager;

    private List<SkillType> lockableSkills = new List<SkillType>();

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
        nextGlitchTime = glitchEffectInterval;
        nextSkillChangeTime = skillChangeInterval;
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

        isActive = false;
        owner.NmAgent.isStopped = false;
        owner.Animator.ResetTrigger("RansomLock");
    }

    public override void Update()
    {
        if (!isActive) return;
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

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnApplySkillLock()
    {
        Debug.Log("[Phase2_RansomLock_State] OnApplySkillLock");

        // ���� ��ų ��� ����
        ApplyLockPattern();
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void OnGlitchEffect()
    {
        Debug.Log("[Phase2_RansomLock_State] OnGlitchEffect");
        PlayGlitchEffect();
    }

    private void InitializeReferences()
    {
        uiManager = GameObject.FindObjectOfType<UIManager>();
        playerControl = GameObject.FindObjectOfType<PlayerControl>();
        playerStatus = GameObject.FindObjectOfType<PlayerStatus>();
        postProcessingManager = GameObject.FindObjectOfType<PostProcessingManager>();
    }

    private void InitializeLockableSkills()
    {
        lockableSkills.Clear();

        lockableSkills.Add(SkillType.Running);
        lockableSkills.Add(SkillType.Jumping);
        lockableSkills.Add(SkillType.Dash);
        lockableSkills.Add(SkillType.Movement);
    }

    private void SetupUI()
    {
        if (uiManager == null) return;

        // UI�� �������� ��� ǥ��
        // uiManager.ShowRansomLockUI(stateDuration);
    }

    private void CleanupUI()
    {
        if (uiManager == null) return;

        // UI���� �������� ��� ǥ�� ����
        // uiManager.HideRansomLockUI();
    }

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

    private void ApplyFixedPattern()
    {
        int count = Mathf.Min(lockableSkills.Count, 2);

        for (int i = 0; i < count; i++)
        {
            owner.LockPlayerSkill(lockableSkills[i], duration);
        }
    }

    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        int cycleIndex = Mathf.FloorToInt(Time.time / skillChangeInterval) % lockableSkills.Count;
        owner.LockPlayerSkill(lockableSkills[cycleIndex], duration);
    }

    private void ApplyPulsatingPattern()
    {
        bool shouldLock = Mathf.FloorToInt(Time.time / skillChangeInterval) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxLockedSkills);

            for (int i = 0; i < count; i++)
            {
                owner.LockPlayerSkill(lockableSkills[i], duration);
            }
        }
    }

    private void ApplyProgressivePattern()
    {
        if (lockableSkills.Count == 0) return;

        int skillsToLock = 0;
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxLockedSkills, lockableSkills.Count));

        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    private void ApplyRandomPattern()
    {
        if (lockableSkills.Count == 0) return;

        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        int skillsToLock = Random.Range(1, Mathf.Min(maxLockedSkills, shuffledSkills.Count) + 1);

        for (int i = 0; i < skillsToLock; i++)
        {
            owner.LockPlayerSkill(shuffledSkills[i], duration);
        }
    }

    private void UpdateGlitchEffects()
    {
        if (Time.time >= nextGlitchTime)
        {
            PlayGlitchEffect();
            nextGlitchTime = Time.time + Random.Range(glitchEffectInterval * 0.8f, glitchEffectInterval * 1.2f);
        }
    }

    private void PlayGlitchEffect()
    {
        if (uiManager != null)
        {
            // uiManager.PlayGlitchEffect(glitchEffectDuration);
        }
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