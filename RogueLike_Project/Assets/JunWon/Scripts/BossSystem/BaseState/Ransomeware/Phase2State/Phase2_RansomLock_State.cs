using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Phase2_RansomLock_State : State<Ransomware>
{
    [Header("��ų�� ����")]
    [SerializeField] private float lockDuration = 60f;      // ��� ���� �ð�(��)
    [SerializeField] private RandomSkillLockController.LockPatternType patternType = RandomSkillLockController.LockPatternType.RandomRotation;

    [Header("�ð� ȿ�� ����")]
    [SerializeField] private float glitchEffectDuration = 1.0f;    // �۸�ġ ȿ�� ���� �ð�
    [SerializeField] private float glitchEffectInterval = 15.0f;   // �۸�ġ ȿ�� ����

    // ���� ����
    private float stateTimer = 0f;
    private float nextGlitchTime = 0f;
    private bool isActive = false;

    // ����
    private RandomSkillLockController skillLockController;
    private UIManager uiManager;
    private PlayerControl playerControl;

    // �̺�Ʈ ���� ����
    private bool isSubscribed = false;

    public Phase2_RansomLock_State(Ransomware owner) : base(owner)
    {
    }

    public override void Enter()
    {
        Debug.Log("Phase2_RansomLock_State: ���� ����");

        // �ʿ��� ������Ʈ ���� ã��
        InitializeReferences();

        // ���� �ʱ�ȭ
        stateTimer = 0f;
        nextGlitchTime = glitchEffectInterval;
        isActive = true;

        // �̺�Ʈ ����
        SubscribeToEvents();

        // UI �ʱ�ȭ
        SetupUI();

        // ��ų�� ����
        StartSkillLock();
    }

    public override void Exit()
    {
        Debug.Log("Phase2_RansomLock_State: ���� ����");

        // ��ų�� ����
        CleanupSkillLock();

        // �̺�Ʈ ���� ����
        UnsubscribeFromEvents();

        // UI ����
        CleanupUI();

        isActive = false;
    }

    public override void Update()
    {
        if (!isActive) return;

        // Ÿ�̸� ������Ʈ
        stateTimer += Time.deltaTime;

        // �۸�ġ ȿ�� ������Ʈ
        UpdateGlitchEffects();

        // ���� ���� Ȯ��
        if (stateTimer >= lockDuration)
        {
            CompleteState();
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    private void InitializeReferences()
    {
        // ��ų�� ��Ʈ�ѷ� ã�ų� ����
        skillLockController = owner.GetComponent<RandomSkillLockController>();
        if (skillLockController == null)
        {
            skillLockController = owner.gameObject.AddComponent<RandomSkillLockController>();
        }

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
    }

    /// <summary>
    /// �̺�Ʈ ����
    /// </summary>
    private void SubscribeToEvents()
    {
        if (isSubscribed) return;

        RandomSkillLockController.OnSkillLockEvent += HandleSkillLockEvent;
        isSubscribed = true;
    }

    /// <summary>
    /// �̺�Ʈ ���� ����
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (!isSubscribed) return;

        RandomSkillLockController.OnSkillLockEvent -= HandleSkillLockEvent;
        isSubscribed = false;
    }

    /// <summary>
    /// UI ����
    /// </summary>
    private void SetupUI()
    {
        if (uiManager == null) return;

        //uiManager.ShowRansomLockUI(lockDuration);
        //uiManager.ShowNotification("�ý����� �����Ǿ����ϴ�! ������� ���� ����� ���ѵ˴ϴ�.");
    }

    /// <summary>
    /// UI ����
    /// </summary>
    private void CleanupUI()
    {
        if (uiManager == null) return;

        //uiManager.HideRansomLockUI();
    }

    /// <summary>
    /// ��ų�� ����
    /// </summary>
    private void StartSkillLock()
    {
        if (skillLockController == null || playerControl == null) return;

        // ��ų�� ��Ʈ�ѷ� ����
        skillLockController.SetTarget(playerControl);

        skillLockController.SetLockPattern(patternType);
        // ���� ����

        // ��ų�� ����
        skillLockController.StartLock(lockDuration);

        // �۸�ġ ȿ�� ���
        PlayGlitchEffect();
    }

    /// <summary>
    /// ��ų�� ����
    /// </summary>
    private void CleanupSkillLock()
    {
        if (skillLockController == null) return;

        skillLockController.StopLock();
    }

    /// <summary>
    /// ���� �Ϸ� �� ���� ���·� ��ȯ
    /// </summary>
    private void CompleteState()
    {
        Debug.Log("�������� ��� ���� �Ϸ�");

        // ���� ���·� ��ȯ
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
        if (uiManager == null) return;

        //uiManager.PlayGlitchEffect(glitchEffectDuration);
    }

    /// <summary>
    /// ��ų�� �̺�Ʈ ó��
    /// </summary>
    private void HandleSkillLockEvent(string eventType, List<SkillType> lockedSkills, float remainingTime)
    {
        if (uiManager == null) return;

        switch (eventType)
        {
            case "LockStarted":
                Debug.Log("��ų�� ���۵�");
                break;

            case "SkillsChanged":
                // ��� ��ų UI ������Ʈ
                //UpdateLockedSkillsUI(lockedSkills);

                // ���� �˸� ǥ��
                string skillNames = GetSkillNamesString(lockedSkills);
                //uiManager.ShowNotification($"��� ���� ����: {skillNames}");

                // �۸�ġ ȿ��
                PlayGlitchEffect();
                break;

            case "LockEnded":
                Debug.Log("��ų�� �����");
                // UI���� ��� ��� ������ ����
                //UpdateLockedSkillsUI(new List<SkillType>());
                break;
        }

        // Ÿ�̸� ������Ʈ
        //uiManager.UpdateLockTimer(remainingTime);
    }

    /// <summary>
    /// ��� ��ų UI ������Ʈ
    /// </summary>
    //private void UpdateLockedSkillsUI(List<SkillType> lockedSkills)
    //{
    //    if (uiManager == null) return;

    //    // ��� ������ �ʱ�ȭ
    //    uiManager.ShowLockedFeatureIcon("running", false);
    //    uiManager.ShowLockedFeatureIcon("jump", false);
    //    uiManager.ShowLockedFeatureIcon("dash", false);
    //    uiManager.ShowLockedFeatureIcon("movement", false);

    //    // ��� ��ų�� ������ ǥ��
    //    foreach (SkillType skill in lockedSkills)
    //    {
    //        switch (skill)
    //        {
    //            case SkillType.Running:
    //                uiManager.ShowLockedFeatureIcon("running", true);
    //                break;

    //            case SkillType.Jumping:
    //                uiManager.ShowLockedFeatureIcon("jump", true);
    //                break;

    //            case SkillType.Dash:
    //                uiManager.ShowLockedFeatureIcon("dash", true);
    //                break;

    //            case SkillType.Movement:
    //                uiManager.ShowLockedFeatureIcon("movement", true);
    //                break;
    //        }
    //    }
    //}

    /// <summary>
    /// ��ų �̸� ��� ���ڿ� ��ȯ
    /// </summary>
    private string GetSkillNamesString(List<SkillType> skills)
    {
        if (skills == null || skills.Count == 0)
        {
            return "����";
        }

        List<string> skillNames = new List<string>();

        foreach (SkillType skill in skills)
        {
            switch (skill)
            {
                case SkillType.Running:
                    skillNames.Add("�޸���");
                    break;

                case SkillType.Jumping:
                    skillNames.Add("����");
                    break;

                case SkillType.Dash:
                    skillNames.Add("���");
                    break;

                case SkillType.Movement:
                    skillNames.Add("�̵�");
                    break;

                default:
                    skillNames.Add(skill.ToString());
                    break;
            }
        }

        return string.Join(", ", skillNames);
    }
}
