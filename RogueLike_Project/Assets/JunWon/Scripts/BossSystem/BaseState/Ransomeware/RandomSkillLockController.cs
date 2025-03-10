using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSkillLockController : MonoBehaviour
{
    [Header("��ų�� ����")]
    [SerializeField] private float defaultLockDuration = 60f;  // �⺻ ��� ���� �ð�(��)
    [SerializeField] private float skillChangeDuration = 10f;  // ��ų ���� �ֱ�(��)

    [Header("��� ���� ����")]
    [SerializeField] private LockPatternType lockPatternType = LockPatternType.RandomRotation;
    [Range(1, 3)]
    [SerializeField] private int maxSimultaneousLockedSkills = 2;  // ���ÿ� ��� �� �ִ� �ִ� ��ų ��

    [Header("��� ������ ��ų ����")]
    [SerializeField] private List<SkillType> lockableSkills = new List<SkillType>();

    [Header("ȿ�� ����")]
    [SerializeField] private AudioClip lockSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip glitchSound;

    // ���� ����
    private ISkillLockable targetObject;
    private AudioSource audioSource;
    private bool isLockActive = false;
    private float lockTimer = 0f;
    private float nextSkillChangeTime = 0f;
    private List<SkillType> currentlyLockedSkills = new List<SkillType>();

    // �̺�Ʈ ����
    public delegate void SkillLockEvent(string eventType, List<SkillType> lockedSkills, float remainingTime);
    public static event SkillLockEvent OnSkillLockEvent;

    // ��ų�� ���� Ÿ��
    public enum LockPatternType
    {
        Fixed,              // ������ ��ų�� ���
        RandomRotation,     // ���������� ���� ��ų ���
        PulsatingLock,      // ��� ��ų �ѹ��� ��� �� ����
        ProgressivelyWorse, // �ð��� �������� �� ���� ��ų ���
        CompletelyRandom    // ���� ����
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // �⺻ ��� ���� ��ų ���� (�������� ���� ���)
        if (lockableSkills.Count == 0)
        {
            lockableSkills.Add(SkillType.Running);
            lockableSkills.Add(SkillType.Jumping);
            lockableSkills.Add(SkillType.Dash);
        }
    }

    /// <summary>
    /// ��ų�� ��� ����
    /// </summary>
    /// <param name="target">ISkillLockable �������̽� ���� ���</param>
    public void SetTarget(ISkillLockable target)
    {
        targetObject = target;
    }
    public void SetLockPattern(LockPatternType pattern)
    {
        lockPatternType = pattern;
    }

    /// <summary>
    /// ��ų�� ����
    /// </summary>
    /// <param name="duration">��� ���� �ð� (��). �⺻�� ��� �� 0 �Ǵ� ���� �Է�</param>
    public void StartLock(float duration = 0)
    {
        if (isLockActive || targetObject == null)
            return;

        // ��ȿ�� ���� �ð� ����
        float lockDuration = duration > 0 ? duration : defaultLockDuration;

        // ��ų�� �ڷ�ƾ ����
        StartCoroutine(SkillLockCoroutine(lockDuration));
    }


    /// <summary>
    /// ��ų�� ���� ����
    /// </summary>
    public void StopLock()
    {
        if (!isLockActive || targetObject == null)
            return;

        StopAllCoroutines();
        EndLock();
    }

    /// <summary>
    /// ��ų�� �ڷ�ƾ
    /// </summary>
    private IEnumerator SkillLockCoroutine(float duration)
    {
        isLockActive = true;
        lockTimer = 0f;
        nextSkillChangeTime = 0f;

        // �ʱ� ��� ����
        ApplyLockPattern();

        // ��ų�� ���� �̺�Ʈ �߻�
        TriggerLockEvent("LockStarted", currentlyLockedSkills, duration);

        // ��� ���� ���
        PlaySound(lockSound);

        Debug.Log($"��ų�� ����: {duration}�� ����");

        while (lockTimer < duration)
        {
            lockTimer += Time.deltaTime;
            float remainingTime = duration - lockTimer;

            // ��ų ���� �ð��� �Ǹ� ���ο� ��ų ��� ���� ����
            if (lockTimer >= nextSkillChangeTime)
            {
                ApplyLockPattern();
                nextSkillChangeTime = lockTimer + skillChangeDuration;

                // ��ų ���� �̺�Ʈ �߻�
                TriggerLockEvent("SkillsChanged", currentlyLockedSkills, remainingTime);

                // �۸�ġ ȿ�� ���
                PlaySound(glitchSound);
            }

            yield return null;
        }

        // ��ų�� ����
        EndLock();
    }

    /// <summary>
    /// ��ų�� ���� ó��
    /// </summary>
    private void EndLock()
    {
        if (targetObject != null)
        {
            // ��� ��ų ��� ����
            targetObject.UnlockAllSkills();
        }

        isLockActive = false;
        currentlyLockedSkills.Clear();

        // ��� ���� ���� ���
        PlaySound(unlockSound);

        // ��ų�� ���� �̺�Ʈ �߻�
        TriggerLockEvent("LockEnded", new List<SkillType>(), 0);

        Debug.Log("��ų�� �����");
    }

    /// <summary>
    /// ������ ���Ͽ� ���� ��ų ��� ����
    /// </summary>
    private void ApplyLockPattern()
    {
        if (targetObject == null || lockableSkills.Count == 0)
            return;

        // ������ ��� ��� ��ų ����
        UnlockAllSkills();
        currentlyLockedSkills.Clear();

        switch (lockPatternType)
        {
            case LockPatternType.Fixed:
                ApplyFixedPattern();
                break;

            case LockPatternType.RandomRotation:
                ApplyRandomRotationPattern();
                break;

            case LockPatternType.PulsatingLock:
                ApplyPulsatingPattern();
                break;

            case LockPatternType.ProgressivelyWorse:
                ApplyProgressivePattern();
                break;

            case LockPatternType.CompletelyRandom:
                ApplyRandomPattern();
                break;
        }

        // ���� ��� ��ų �α� ���
        LogLockedSkills();
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
            LockSkill(lockableSkills[i]);
        }
    }

    /// <summary>
    /// ���� ��ȯ ���� - �Ź� �ٸ� ��ų�� ��ȯ�ϸ� ���
    /// </summary>
    private void ApplyRandomRotationPattern()
    {
        if (lockableSkills.Count == 0) return;

        // ��ȯ ��ġ ��� (��ȯ �ֱ�� lockableSkills ����)
        int cycleIndex = Mathf.FloorToInt(lockTimer / skillChangeDuration) % lockableSkills.Count;

        // �ش� �ε����� ��ų ���
        LockSkill(lockableSkills[cycleIndex]);
    }

    /// <summary>
    /// �ƹ� ���� - ��� ��ų ��� �� ������ �ݺ�
    /// </summary>
    private void ApplyPulsatingPattern()
    {
        // ¦�� �ֱ⿡�� ���, Ȧ�� �ֱ⿡�� �ƹ��͵� ����� ����
        bool shouldLock = Mathf.FloorToInt(lockTimer / skillChangeDuration) % 2 == 0;

        if (shouldLock)
        {
            int count = Mathf.Min(lockableSkills.Count, maxSimultaneousLockedSkills);

            for (int i = 0; i < count; i++)
            {
                LockSkill(lockableSkills[i]);
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
        float progress = lockTimer / defaultLockDuration;
        int skillsToLock = Mathf.CeilToInt(progress * maxSimultaneousLockedSkills);
        skillsToLock = Mathf.Clamp(skillsToLock, 1, Mathf.Min(maxSimultaneousLockedSkills, lockableSkills.Count));

        // ���纻 ���� �� ����
        List<SkillType> shuffledSkills = new List<SkillType>(lockableSkills);
        ShuffleList(shuffledSkills);

        // ���� ����ŭ ��ų ���
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
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
        int skillsToLock = UnityEngine.Random.Range(1, Mathf.Min(maxSimultaneousLockedSkills, shuffledSkills.Count) + 1);

        // ���õ� ����ŭ ��ų ���
        for (int i = 0; i < skillsToLock; i++)
        {
            LockSkill(shuffledSkills[i]);
        }
    }


    private void LockSkill(SkillType skillType)
    {
        if (targetObject == null) return;

        targetObject.SetSkillEnabled(skillType, false);
        currentlyLockedSkills.Add(skillType);
    }

 
    private void UnlockAllSkills()
    {
        if (targetObject == null) return;

        foreach (SkillType skill in lockableSkills)
        {
            targetObject.SetSkillEnabled(skill, true);
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

    /// <summary>
    /// ����Ʈ ���� (Fisher-Yates �˰���)
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + UnityEngine.Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private void TriggerLockEvent(string eventType, List<SkillType> skills, float remainingTime)
    {
        OnSkillLockEvent?.Invoke(eventType, new List<SkillType>(skills), remainingTime);
    }
}
