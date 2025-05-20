using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region �� ���� ����
    [Header("�� ������Ʈ")]
    [SerializeField] private GameObject basicFormObject;     // �⺻ �� ������Ʈ (�ڱ� �ڽ�)
    [SerializeField] private GameObject wormFormObject;      // �� �� ������Ʈ (�ڽ�)
    [SerializeField] private GameObject trojanFormObject;    // Ʈ���� �� �� ������Ʈ (�ڽ�)
    [SerializeField] private GameObject ransomwareFormObject; // �������� �� ������Ʈ (�ڽ�)

    // �� �� ������Ʈ ĳ��
    private WormBossPrime wormComponent;
    private Troy trojanComponent;
    private Ransomware ransomwareComponent;

    // ���� Ȱ��ȭ�� �� ����
    private GameObject currentActiveFormObject;
    private BossBase currentActiveBoss;
    private bool isTransforming = false;
    private BossForm currentForm = BossForm.Basic;

    // �� ���� ��Ÿ�� ����
    private float lastFormChangeTime = 0f;
    private float formStayDuration = 15f; // ������ ���� �ӹ��� �ð�
    private float formTimer = 0f;
    #endregion

    #region ���� ����
    [Header("���� ����")]
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseAttackRange = 10f;
    [SerializeField] private float baseAttackCooldown = 3f;

    [Header("�� ����")]
    [SerializeField] private GameObject mapAttackVFX;
    [SerializeField] private float mapAttackCooldown = 15f;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.8f;
    private float lastMapAttackTime = 0f;

    [Header("�� ����")]
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private float transformationTime = 3f;
    [SerializeField] private float formChangeCooldown = 30f;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;

    [Header("������Ʈ")]
    [SerializeField] private AbilityManager abilityManager;
    #endregion

    #region ���� �ӽ�
    [Header("���� �ӽ�")]
    [SerializeField] private StateMachine<UnknownVirusBoss> fsm;

    // ���µ�
    private IntroState_UnknownVirus introState;
    private BasicCombatState_UnknownVirus basicState;
    private MapAttackState_UnknownVirus mapAttackState;
    private TransformState_UnknownVirus transformState;
    private WormCombatState_UnknownVirus wormCombatState;
    private TrojanCombatState_UnknownVirus trojanCombatState;
    private RansomwareCombatState_UnknownVirus ransomwareCombatState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region ���� ���� �޼���

    public void SetMapAttackState(MapAttackState_UnknownVirus state)
    {
        mapAttackState = state;
    }

    public void SetTransformState(TransformState_UnknownVirus state)
    {
        transformState = state;
    }
    #endregion


    #region �� ���� ����
    [Header("�� ���� ����")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int attackAreaSize = 5; // ���� ���� ũ�� (5x5)
    [SerializeField] private float tileSearchInterval = 0.1f; // Ÿ�� �˻� ����
    [SerializeField] private float shockwavePower = 30f; // ����� ����
    [SerializeField] private LayerMask playerLayer;
    #endregion

    #region ���� ������Ƽ
    public BossForm CurrentForm => currentForm;
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public BossStatus MonsterStatus => bossStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    public BossBase GetCurrentActiveBoss() => currentActiveBoss;
    #endregion

    #region ����Ƽ ����������Ŭ
    private void Start()
    {
        InitializeComponents();
        InitializeFormHierarchy();
        InitializeAbilities();
        InitializeStates();
        InitializeFSM();

        Debug.Log("[UnknownVirusBoss] �ʱ�ȭ �Ϸ�");
    }

    private void Update()
    {
        // FSM ������Ʈ
        fsm.Update();

        // ���� Ȱ��ȭ�� ���� ü���� ���� ������ ����ȭ
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            SyncHealthFromActiveBoss();
        }

        // ���� ���� ������Ʈ
        UpdateActiveFormLogic();

        // �� ���� Ÿ�̸� üũ (���� ���� �ӹ��� �ð�)
        CheckFormTransformationTimer();

        // ��� ���� Ȯ��
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region �ʱ�ȭ �޼���
    private void InitializeComponents()
    {
        // ���� ã��
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;

        // ������Ʈ ��������
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();

        Debug.Log("[UnknownVirusBoss] ������Ʈ �ʱ�ȭ �Ϸ�");
    }

    private void InitializeFormHierarchy()
    {
        // �� ������Ʈ ��ȿ�� �˻�
        if (basicFormObject == null)
        {
            Debug.LogError("[UnknownVirusBoss] �⺻ �� ������Ʈ�� �����ϴ�!");
            return;
        }

        // �� ������Ʈ ĳ��
        if (wormFormObject != null)
        {
            wormComponent = wormFormObject.GetComponent<WormBossPrime>();
            wormFormObject.SetActive(false);
        }

        if (trojanFormObject != null)
        {
            trojanComponent = trojanFormObject.GetComponent<Troy>();
            trojanFormObject.SetActive(false);
        }

        if (ransomwareFormObject != null)
        {
            ransomwareComponent = ransomwareFormObject.GetComponent<Ransomware>();
            ransomwareFormObject.SetActive(false);
        }

        // �ʱ� ���� - �⺻ ���� Ȱ��ȭ
        ActivateBasicFormOnly();

        Debug.Log("[UnknownVirusBoss] �� ���� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeAbilities()
    {
        // �� ���� �ɷ� Ȱ��ȭ
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        Debug.Log("[UnknownVirusBoss] �ɷ� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeStates()
    {
        introState = new IntroState_UnknownVirus(this);
        basicState = new BasicCombatState_UnknownVirus(this);
        mapAttackState = new MapAttackState_UnknownVirus(this);
        transformState = new TransformState_UnknownVirus(this);
        wormCombatState = new WormCombatState_UnknownVirus(this);
        trojanCombatState = new TrojanCombatState_UnknownVirus(this);
        ransomwareCombatState = new RansomwareCombatState_UnknownVirus(this);
        deadState = new DefeatedState_UnknownVirus(this);

        Debug.Log("[UnknownVirusBoss] ���� �ʱ�ȭ �Ϸ�");
    }

    private void InitializeFSM()
    {
        // ���� �ν��Ͻ� ����
        var states = CreateStates();

        // �ʱ� ���¸� ��Ʈ�η� ������ FSM ����
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // ���� ����
        SetupTransitions(states);

        Debug.Log("[UnknownVirusBoss] FSM �ʱ�ȭ �Ϸ�");
    }

    private (
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        WormCombatState_UnknownVirus wormCombatState,
        TrojanCombatState_UnknownVirus trojanCombatState,
        RansomwareCombatState_UnknownVirus ransomwareCombatState,
        DefeatedState_UnknownVirus deadState
    ) CreateStates()
    {
        return (
            new IntroState_UnknownVirus(this),
            new BasicCombatState_UnknownVirus(this),
            new MapAttackState_UnknownVirus(this),
            new TransformState_UnknownVirus(this),
            new WormCombatState_UnknownVirus(this),
            new TrojanCombatState_UnknownVirus(this),
            new RansomwareCombatState_UnknownVirus(this),
            new DefeatedState_UnknownVirus(this)
        );
    }

    private void SetupTransitions((
        IntroState_UnknownVirus introState,
        BasicCombatState_UnknownVirus basicState,
        MapAttackState_UnknownVirus mapAttackState,
        TransformState_UnknownVirus transformState,
        WormCombatState_UnknownVirus wormCombatState,
        TrojanCombatState_UnknownVirus trojanCombatState,
        RansomwareCombatState_UnknownVirus ransomwareCombatState,
        DefeatedState_UnknownVirus deadState
    ) s)
    {
        // ��Ʈ�� �� �⺻ ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // �⺻ ���� �� �� ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 &&
                 UnityEngine.Random.value < mapAttackChance));

        // �� ���� �� �⺻ ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => mapAttackState.IsAnimationFinished()
        ));

        // �⺻ ���� �� ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => Time.time - lastFormChangeTime >= formChangeCooldown &&
                 UnityEngine.Random.value < formChangeChance));

        // ���� �� �� �� ���� ����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.basicState,
            () => !isTransforming && currentForm == BossForm.Basic));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.wormCombatState,
            () => !isTransforming && currentForm == BossForm.Worm));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.trojanCombatState,
            () => !isTransforming && currentForm == BossForm.Trojan));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.transformState, s.ransomwareCombatState,
            () => !isTransforming && currentForm == BossForm.Ransomware));

        // �� �� ���� �� �⺻ ����
        // ����: ���� ������ �⺻ ������ ���ƿ��� ���� Update���� Ÿ�̸ӷ� ó����
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.wormCombatState, s.transformState,
            () => formTimer >= formStayDuration));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.trojanCombatState, s.transformState,
            () => formTimer >= formStayDuration));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.ransomwareCombatState, s.transformState,
            () => formTimer >= formStayDuration));

        // ���� ��� ���� ���� (��Ʈ�� ����)
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { s.introState };
        fsm.AddGlobalTransition(s.deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }
    #endregion



    #region �� ���� �޼���
    private void ActivateBasicFormOnly()
    {
        // �⺻ ���� Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ
        if (basicFormObject != null)
            basicFormObject.SetActive(true);

        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // ���� �� ����
        currentForm = BossForm.Basic;
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }

    public void ApplyForm(BossForm form)
    {
        // �� ��ȯ ����
        if (form == currentForm) return;

        // ���� Ȱ��ȭ�� �� ��Ȱ��ȭ
        DeactivateCurrentForm();

        // �� �� Ȱ��ȭ
        ActivateForm(form);

        // �� Ÿ�̸� ����
        formTimer = 0f;

        // ���� �� ������Ʈ
        currentForm = form;

        // �� ���� �� �ɷ� ������Ʈ
        UpdateFormSpecificAbilities();

        Debug.Log($"[UnknownVirusBoss] {form} ������ ���� �Ϸ�");
    }

    private void DeactivateCurrentForm()
    {
        // ���� Ȱ��ȭ�� �� ������Ʈ ��Ȱ��ȭ
        if (currentActiveFormObject != null)
        {
            // ü�� ���� ����ȭ
            SyncHealthFromActiveBoss();

            // �� ������Ʈ ��Ȱ��ȭ
            currentActiveFormObject.SetActive(false);
        }

        // ���� Ȱ�� ���� ���� �ʱ�ȭ
        currentActiveBoss = null;
    }

    private void ActivateForm(BossForm form)
    {
        GameObject targetFormObject = null;

        // ���� ���� ��� ������Ʈ ����
        switch (form)
        {
            case BossForm.Basic:
                targetFormObject = basicFormObject;
                currentActiveBoss = null;
                break;

            case BossForm.Worm:
                targetFormObject = wormFormObject;
                currentActiveBoss = wormComponent;
                break;

            case BossForm.Trojan:
                targetFormObject = trojanFormObject;
                currentActiveBoss = trojanComponent;
                break;

            case BossForm.Ransomware:
                targetFormObject = ransomwareFormObject;
                currentActiveBoss = ransomwareComponent;
                break;
        }

        // ��� �� ������Ʈ Ȱ��ȭ
        if (targetFormObject != null)
        {
            // ��ġ/ȸ�� ����ȭ
            SyncFormTransform(targetFormObject);

            // ü�� ����ȭ
            SyncHealthToActiveForm(targetFormObject, form);

            // Ȱ��ȭ
            targetFormObject.SetActive(true);

            // ���� Ȱ�� �� ������Ʈ ������Ʈ
            currentActiveFormObject = targetFormObject;
        }
    }

    private void SyncFormTransform(GameObject formObject)
    {
        if (formObject == null || formObject == basicFormObject)
            return;

        // �⺻ �� ��ġ�� �������� ����ȭ
        formObject.transform.position = transform.position;
        formObject.transform.rotation = transform.rotation;
    }

    private void SyncHealthToActiveForm(GameObject formObject, BossForm form)
    {
        // ���� ü�� ���� ���
        float healthRatio = bossStatus.GetHealth() / bossStatus.GetMaxHealth();

        // ��� ���� ü�� ������Ʈ ��������
        BossStatus targetStatus = null;

        switch (form)
        {
            case BossForm.Worm:
                if (wormComponent != null)
                    targetStatus = wormComponent.BossStatus;
                break;

            case BossForm.Trojan:
                if (trojanComponent != null)
                    targetStatus = trojanComponent.BossStatus;
                break;

            case BossForm.Ransomware:
                if (ransomwareComponent != null)
                    targetStatus = ransomwareComponent.BossStatus;
                break;
        }

        // ��� ü�� ���� (���� �����ϰ�)
        if (targetStatus != null)
        {
            float newHealth = targetStatus.GetMaxHealth() * healthRatio;
            targetStatus.SetHealth(newHealth);
        }
    }

    private void SyncHealthFromActiveBoss()
    {
        if (currentForm == BossForm.Basic || currentActiveBoss == null)
            return;

        // ���� Ȱ�� ���� ü�� ���� ���
        BossStatus formStatus = currentActiveBoss.GetComponent<BossStatus>();
        if (formStatus == null)
            return;

        float healthRatio = formStatus.GetHealth() / formStatus.GetMaxHealth();

        // ��ü ü�� ���� ����ȭ
        bossStatus.SetHealth(bossStatus.GetMaxHealth() * healthRatio);

        // UI ����
        HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
    }

    private void UpdateFormSpecificAbilities()
    {
        // ���� ���� ���� Ư�� �ɷ� Ȱ��ȭ/��Ȱ��ȭ
        switch (currentForm)
        {
            case BossForm.Basic:
                abilityManager.SetAbilityActive("BasicAttack");
                abilityManager.SetAbilityActive("MapAttack");
                break;

            case BossForm.Worm:
                // �� �� Ưȭ �ɷ� ���� (�ʿ��)
                break;

            case BossForm.Trojan:
                // Ʈ���� �� �� Ưȭ �ɷ� ���� (�ʿ��)
                break;

            case BossForm.Ransomware:
                // �������� �� Ưȭ �ɷ� ���� (�ʿ��)
                break;
        }
    }

    private void DeactivateAllForms()
    {
        // ��� �� ��Ȱ��ȭ (����� ���)
        if (wormFormObject != null)
            wormFormObject.SetActive(false);

        if (trojanFormObject != null)
            trojanFormObject.SetActive(false);

        if (ransomwareFormObject != null)
            ransomwareFormObject.SetActive(false);

        // ���� �ʱ�ȭ
        currentActiveFormObject = basicFormObject;
        currentActiveBoss = null;
    }
    #endregion

    #region ������Ʈ ����
    private void UpdateActiveFormLogic()
    {
        // ���� ���� ���� Ư�� ���� ������Ʈ
        switch (currentForm)
        {
            case BossForm.Basic:
                // �⺻ �������� ����
                UpdateBasicFormLogic();
                break;

            case BossForm.Worm:
                // �� �������� ����
                UpdateWormFormLogic();
                break;

            case BossForm.Trojan:
                // Ʈ���� �� �������� ����
                UpdateTrojanFormLogic();
                break;

            case BossForm.Ransomware:
                // �������� �������� ����
                UpdateRansomwareFormLogic();
                break;
        }
    }

    private void UpdateBasicFormLogic()
    {
        // �⺻ �������� ���� �� �ǻ���� ó��
        if (fsm.CurrentState is BasicCombatState_UnknownVirus)
        {
            // �⺻ ���� �� ���� ���� ó�� (�ʿ��)

            // �� ���� ���� ���� (�ֱ������� üũ)
            if (Time.time - lastFormChangeTime >= formChangeCooldown &&
                UnityEngine.Random.value < formChangeChance * Time.deltaTime * 5f) // Ȯ�� ����
            {
                DecideFormTransformation();
            }
        }
    }

    private void UpdateWormFormLogic()
    {
        // �� �� Ưȭ ���� (�ʿ��)
        if (wormComponent != null && wormComponent.isActiveAndEnabled)
        {
            // �߰� ������ �ʿ��ϸ� ���⿡ ����
        }
    }

    private void UpdateTrojanFormLogic()
    {
        // Ʈ���� �� �� Ưȭ ���� (�ʿ��)
        if (trojanComponent != null && trojanComponent.isActiveAndEnabled)
        {
            // �߰� ������ �ʿ��ϸ� ���⿡ ����
        }
    }

    private void UpdateRansomwareFormLogic()
    {
        // �������� �� Ưȭ ���� (�ʿ��)
        if (ransomwareComponent != null && ransomwareComponent.isActiveAndEnabled)
        {
            // �߰� ������ �ʿ��ϸ� ���⿡ ����
        }
    }

    private void CheckFormTransformationTimer()
    {
        // ���� ���� �ӹ��� �ð� üũ
        if (currentForm != BossForm.Basic && !isTransforming)
        {

            // ������ �ð� �̻� �������� �⺻ ������ ���� �غ�
            if (formTimer >= formStayDuration &&
                !(fsm.CurrentState is TransformState_UnknownVirus))
            {
                PrepareToReturnToBasicForm();
            }
        }
    }

    private void PrepareToReturnToBasicForm()
    {
        // �⺻ ������ ���� ��ȯ ����
        isTransforming = true;
        lastFormChangeTime = Time.time;

        // ���� ���·� ��ȯ
        fsm.ForcedTransition(transformState);

        // �⺻ ������ ���� ��û
        RequestFormChange(BossForm.Basic);
    }

    private void DecideFormTransformation()
    {
        // ���� ���� �� ���� (����)
        int formIndex = UnityEngine.Random.Range(1, 4); // 1~3 (Worm, Trojan, Ransomware)
        BossForm nextForm = (BossForm)formIndex;

        // ������ �� ������Ʈ�� ������ �ٸ� �� ����
        switch (nextForm)
        {
            case BossForm.Worm:
                if (wormFormObject == null) nextForm = BossForm.Trojan;
                break;
            case BossForm.Trojan:
                if (trojanFormObject == null) nextForm = BossForm.Ransomware;
                break;
            case BossForm.Ransomware:
                if (ransomwareFormObject == null) nextForm = BossForm.Worm;
                break;
            default:
                if (basicFormObject == null) nextForm = BossForm.Basic;
                break;
        }

        // ������ ���� ������ �⺻ �� ����
        switch (nextForm)
        {
            case BossForm.Worm:
                if (wormFormObject == null) return;
                break;
            case BossForm.Trojan:
                if (trojanFormObject == null) return;
                break;
            case BossForm.Ransomware:
                if (ransomwareFormObject == null) return;
                break;
            default:
                if (basicFormObject == null) return;
                break;
        }

        // ���� ����
        lastFormChangeTime = Time.time;

        // ���� ���·� ��ȯ
        fsm.ForcedTransition(transformState);

        // ���� ��û
        RequestFormChange(nextForm);
    }

    private void HandleDeath()
    {
        // ��� ó��
        DeactivateAllForms();

        // ��� ���·� ��ȯ
        fsm.ForcedTransition(deadState);

        Debug.Log("[UnknownVirusBoss] ���� ���");
    }
    #endregion

    #region ���� ����
    /// <summary>TransformState���� ȣ��</summary>
    public void RequestFormChange(BossForm newForm)
    {
        if (isTransforming) return;

        isTransforming = true;
        StartCoroutine(TransformRoutine(newForm));
    }

    private IEnumerator TransformRoutine(BossForm newForm)
    {
        // ���� ȿ�� Ȱ��ȭ
        if (transformationVFX != null)
            transformationVFX.SetActive(true);

        Debug.Log($"[UnknownVirusBoss] {newForm} ������ ���� ����");

        // ���� �ð� ���
        yield return new WaitForSeconds(transformationTime);

        // �ش� �� ����
        ApplyForm(newForm);

        // ���� ȿ�� ��Ȱ��ȭ
        if (transformationVFX != null)
            transformationVFX.SetActive(false);

        // ���� �Ϸ�
        isTransforming = false;

        // TransformState�� ���� �Ϸ� �˸�
        if (transformState != null)
            transformState.OnTransformationComplete();

        Debug.Log($"[UnknownVirusBoss] {newForm} ������ ���� �Ϸ�");
    }
    #endregion

    #region ������ ó��
    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // ���� �߿� ������ ����
        if (isTransforming)
            return;

        // ���� Ȱ��ȭ�� ���� ������ ����
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            // ���� ���� ������ ����
            currentActiveBoss.TakeDamage(damage, showDamage);

            // �⺻ ������ ������ ����ȭ
            SyncHealthFromActiveBoss();
        }
        else
        {
            // �⺻ �� ������ ó��
            bossStatus.DecreaseHealth(damage);

            // ���� �̺�Ʈ �� UI ǥ��
            EventManager.Instance.TriggerMonsterDamagedEvent();
            if (showDamage && UIDamaged != null)
            {
                var popup = Instantiate(
                    UIDamaged,
                    transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height / 2), 0),
                    Quaternion.identity
                ).GetComponent<UIDamage>();

                popup.damage = damage;
            }

            // UI ����
            HPBar?.SetRatio(bossStatus.GetHealth(), bossStatus.GetMaxHealth());
        }

        // ��� üũ
        if (bossStatus.GetHealth() <= 0 && !(fsm.CurrentState is DefeatedState_UnknownVirus))
        {
            HandleDeath();
        }
    }
    #endregion

    #region �ִϸ��̼� �̺�Ʈ �ڵ鷯
    // �� ���� �ִϸ��̼� �Ϸ� �̺�Ʈ
    public void OnMapAttackFinished()
    {
        if (mapAttackState != null)
        {
            mapAttackState.OnAttackFinished();
        }
    }

    // �⺻ ���� �ִϸ��̼� �Ϸ� �̺�Ʈ
    public void OnBasicAttackFinished()
    {
        // �⺻ ���� �Ϸ� ó�� (�ʿ��)
    }
    #endregion

    #region �� ���� ����
    // �� ���� Ʈ���� �޼���
    public void TriggerMapAttack()
    {
        try
        {
            lastMapAttackTime = Time.time;

            if (tileManager == null)
            {
                Debug.LogError("�� ���� ���� �Ұ�: TileManager�� null�Դϴ�");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // �˻� �˰��� ���� (����)
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));

            mapAttackState?.OnAttackFinished();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TriggerMapAttack ����: {e.Message}\n{e.StackTrace}");
            mapAttackState?.OnAttackFinished();
        }
    }

    private IEnumerator ExecuteMapAttack(int searchMethod)
    {
        // �÷��̾� �ֺ� ��ǥ ���
        Vector3 playerPos = target.position;

        // ���� ��ǥ�� Ÿ�� �׸��� ��ǥ�� ��ȯ
        int centerX = Mathf.RoundToInt(playerPos.x / 2);
        int centerZ = Mathf.RoundToInt(playerPos.z / 2);

        Debug.Log($"�׸��� ��ġ [{centerX}, {centerZ}]���� �� ���� ����");

        // ��ǥ Ÿ�� ������ ���� (���� ���� ��)
        int targetX = centerX + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);
        int targetZ = centerZ + UnityEngine.Random.Range(-attackAreaSize / 2, attackAreaSize / 2 + 1);

        // ��ȿ�� �� ���� ���� ����
        targetX = Mathf.Clamp(targetX, 0, tileManager.GetMapSize - 1);
        targetZ = Mathf.Clamp(targetZ, 0, tileManager.GetMapSize - 1);

        // �˻� ����� ���� ȿ�� ����
        switch (searchMethod)
        {
            case 0:
                yield return StartCoroutine(LinearTileSearch(centerX, centerZ, targetX, targetZ));
                break;
            case 1:
                yield return StartCoroutine(BinaryTileSearch(centerX, centerZ, targetX, targetZ));
                break;
            case 2:
                yield return StartCoroutine(RandomTileSearch(centerX, centerZ, targetX, targetZ));
                break;
        }

        // ���� �Ϸ� �� ���
        yield return new WaitForSeconds(1f);

        // �� ���� ���� �Ϸ� �˸�
        mapAttackState?.OnAttackFinished();
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator LinearTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ��� (�÷��̾� �߽����� attackAreaSize x attackAreaSize ����)
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{minX},{minZ}] ���� [{maxX},{maxZ}], ��ǥ: [{targetX},{targetZ}]");

        // ��� Ÿ���� ���������� �˻�
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                // ���� �˻� Ÿ�� ǥ��
                HighlightTile(x, z, Color.red);

                // ���� ��ǥ�� ��ȯ
                Vector3 tilePos = new Vector3(x * 2, 0, z * 2);

                // ������ ȿ��
                if (laserPrefab != null)
                {
                    GameObject laser = Instantiate(laserPrefab,
                       tilePos + Vector3.up * 0.2f, // Ÿ�� �ٷ� ��
                       Quaternion.identity);

                    // ������ ������ ���� (VirusLaser ������Ʈ�� �ִٰ� ����)
                    var virusLaser = laser.GetComponent<VirusLaser>();
                    if (virusLaser != null)
                    {
                        virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                    }
                }

                yield return new WaitForSeconds(tileSearchInterval);

                // Ÿ�� Ÿ���� ã���� ����� ȿ��
                if (x == targetX && z == targetZ)
                {
                    // Ÿ�� Ÿ�� ǥ��
                    HighlightTile(x, z, Color.green);

                    // ����� ���� (TileManager�� CreateShockwave �ڷ�ƾ ȣ��)
                    StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));

                    // �ش� ��ġ �ֺ��� ������ ����
                    ApplyDamageAroundPosition(new Vector3(x * 2, 0, z * 2));

                    yield break;
                }

                // �˻� �Ϸ�� Ÿ�� ����
                ResetTileColor(x, z);
            }
        }
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator BinaryTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ���
        int leftX = Mathf.Max(0, centerX - halfSize);
        int rightX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int topZ = Mathf.Max(0, centerZ - halfSize);
        int bottomZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{leftX},{topZ}] ���� [{rightX},{bottomZ}], ��ǥ: [{targetX},{targetZ}]");

        int iterations = 0;
        int maxIterations = 10; // ���� ���� ����

        while (leftX <= rightX && topZ <= bottomZ && iterations < maxIterations)
        {
            iterations++;
            int midX = (leftX + rightX) / 2;
            int midZ = (topZ + bottomZ) / 2;

            // ���� �˻� ���� ����
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    HighlightTile(x, z, Color.red);

                    // ��� Ÿ�Ͽ��� ������ ȿ��
                    if (x == leftX || x == rightX || z == topZ || z == bottomZ)
                    {
                        if (laserPrefab != null)
                        {
                            Vector3 tilePos = new Vector3(x * 2, 0, z * 2);
                            GameObject laser = Instantiate(laserPrefab,
                                tilePos + Vector3.up * 0.2f,
                                Quaternion.identity);

                            // ������ ������ ����
                            var virusLaser = laser.GetComponent<VirusLaser>();
                            if (virusLaser != null)
                            {
                                virusLaser.SetDamage(abilityManager.GetAbiltiyDmg("MapAttack"));
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            // ���� ȿ�� �ʱ�ȭ
            for (int x = leftX; x <= rightX; x++)
            {
                for (int z = topZ; z <= bottomZ; z++)
                {
                    ResetTileColor(x, z);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // Ÿ���� ã�Ҵ��� Ȯ��
            if (midX == targetX && midZ == targetZ)
            {
                HighlightTile(midX, midZ, Color.green);
                StartCoroutine(tileManager.CreateShockwave(midX, midZ, halfSize, shockwavePower));
                ApplyDamageAroundPosition(new Vector3(midX * 2, 0, midZ * 2));
                yield break;
            }

            // ���� �˻� ����
            if (targetX < midX)
                rightX = midX - 1;
            else
                leftX = midX + 1;

            if (targetZ < midZ)
                bottomZ = midZ - 1;
            else
                topZ = midZ + 1;
        }

        // Ÿ�� ���� ���� �� ȿ�� ���� (���� �˻��� �������� ���)
        HighlightTile(targetX, targetZ, Color.green);
        StartCoroutine(tileManager.CreateShockwave(targetX, targetZ, halfSize, shockwavePower));
        ApplyDamageAroundPosition(new Vector3(targetX * 2, 0, targetZ * 2));
    }

    // ���� Ÿ�� �˻� ����
    private IEnumerator RandomTileSearch(int centerX, int centerZ, int targetX, int targetZ)
    {
        int halfSize = attackAreaSize / 2;

        // �˻� ���� ���
        int minX = Mathf.Max(0, centerX - halfSize);
        int maxX = Mathf.Min(tileManager.GetMapSize - 1, centerX + halfSize);
        int minZ = Mathf.Max(0, centerZ - halfSize);
        int maxZ = Mathf.Min(tileManager.GetMapSize - 1, centerZ + halfSize);

        Debug.Log($"���� �˻� ����: [{minX},{minZ}] ���� [{maxX},{maxZ}], ��ǥ: [{targetX},{targetZ}]");

        HashSet<Vector2Int> searchedTiles = new HashSet<Vector2Int>();
        int maxAttempts = Mathf.Min(20, (maxX - minX + 1) * (maxZ - minZ + 1));

        for (int i = 0; i < maxAttempts; i++)
        {
            // �˻� ���� ������ ���� Ÿ�� ����
            int x = UnityEngine.Random.Range(minX, maxX + 1);
            int z = UnityEngine.Random.Range(minZ, maxZ + 1);
            Vector2Int tilePos = new Vector2Int(x, z);

            // �̹� �˻��� Ÿ���̸� �ٽ� ���� (�ִ� 3��)
            int attempts = 0;
            while (searchedTiles.Contains(tilePos) && attempts < 3)
            {
                x = UnityEngine.Random.Range(minX, maxX + 1);
                z = UnityEngine.Random.Range(minZ, maxZ + 1);
                tilePos = new Vector2Int(x, z);
                attempts++;
            }

            searchedTiles.Add(tilePos);

            // Ÿ�� ���� �� ������ ȿ��
            HighlightTile(x, z, Color.red);

            if (laserPrefab != null)
            {
                Vector3 worldPos = new Vector3(x * 2, 0.1f, z * 2);
                GameObject laser = Instantiate(laserPrefab,
                    worldPos + Vector3.up * 2,
                    Quaternion.identity);
                Destroy(laser, 2f);
            }

            yield return new WaitForSeconds(tileSearchInterval);

            // Ÿ�� Ÿ���� ã�Ҵ��� Ȯ��
            if (x == targetX && z == targetZ)
            {
                HighlightTile(x, z, Color.green);
                StartCoroutine(tileManager.CreateShockwave(x, z, halfSize, shockwavePower));
                ApplyDamageAroundPosition(new Vector3(x * 2, 0, z * 2));
                yield break;
            }

            // �˻� �Ϸ�� Ÿ�� ����
            ResetTileColor(x, z);
        }

        // �ִ� �õ� Ƚ���� �ʰ��ص� Ÿ���� ã�� ���� ���
        HighlightTile(targetX, targetZ, Color.green);
        StartCoroutine(tileManager.CreateShockwave(targetX, targetZ, halfSize, shockwavePower));
        ApplyDamageAroundPosition(new Vector3(targetX * 2, 0, targetZ * 2));
    }

    // Ÿ�� ���� ���� �޼���
    private void HighlightTile(int x, int z, Color color)
    {
        if (x < 0 || x >= tileManager.GetMapSize || z < 0 || z >= tileManager.GetMapSize)
            return;

        Tile tile = tileManager.GetTiles[z, x];
        if (tile != null && tile.IsSetActive)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", color);
            }
        }
    }

    // Ÿ�� ���� ���� ���� �޼���
    private void ResetTileColor(int x, int z)
    {
        if (x < 0 || x >= tileManager.GetMapSize || z < 0 || z >= tileManager.GetMapSize)
            return;

        Tile tile = tileManager.GetTiles[z, x];
        if (tile != null && tile.IsSetActive)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_BaseColor", Color.white);
            }
        }
    }

    // ������ ��ġ �ֺ��� ������ ����
    private void ApplyDamageAroundPosition(Vector3 centerPosition)
    {
        float damageRadius = attackAreaSize * 1.0f; // ������ �ݰ�

        Debug.Log("������ ����");

        // ������ ����
        Collider[] hitColliders = Physics.OverlapSphere(centerPosition, damageRadius, playerLayer);
        foreach (var collider in hitColliders)
        {
            if (collider == null) continue;

            Debug.Log(collider + "�� �¾ҽ��ϴ�");

            PlayerStatus playerStatus = collider.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.DecreaseHealth(abilityManager.GetAbiltiyDmg("MapAttack"));

                // �߰� ȿ�� - �˹�
                Rigidbody playerRb = collider.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDir = (collider.transform.position - centerPosition).normalized;
                    knockbackDir.y = 0.3f; // �ణ ����
                    playerRb.AddForce(knockbackDir * 10f, ForceMode.Impulse);
                }
            }
        }
    }

    // ���� ���� �� ���� �޼���
    public void CleanupMapAttack()
    {
        // Ȥ�� �����ִ� ������ ����Ʈ ã�Ƽ� ����
        GameObject[] lasers = GameObject.FindGameObjectsWithTag("VirusLaser");
        foreach (var laser in lasers)
        {
            if (laser != null) Destroy(laser);
        }
    }
    #endregion
}