using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class UnknownVirusBoss : BossBase
{
    public enum BossForm { Basic, Worm, Trojan, Ransomware }

    #region Inspector Settings
    [Header("Form Prefabs")]
    [SerializeField] private WormBossPrime wormFormPrefab;
    [SerializeField] private Troy trojanFormPrefab;
    [SerializeField] private Ransomware ransomwareFormPrefab;

    [Header("Basic Combat")]
    [SerializeField] private GameObject originalAttackPrefab;
    [SerializeField] private float originalAttackRange = 10f;
    [SerializeField] private float originalAttackDamage = 25f;
    [SerializeField] private float originalAttackCooldown = 3f;

    [Header("Map Attack")]
    [SerializeField] private GameObject mapAttackVFX;
    [SerializeField] private float mapAttackCooldown = 15f;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.1f;

    [Header("Form Change")]
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private float transformationTime = 3f;
    [SerializeField] private float formChangeCooldown = 30f;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;
    #endregion

    #region Status & State
    [Header("State")]
    // FSM & States
    private StateMachine<UnknownVirusBoss> fsm;
    private IntroState_UnknownVirus introState;
    private BasicCombatState_UnknownVirus basicState;
    private MapAttackState_UnknownVirus mapAttackState;
    private TransformState_UnknownVirus transformState;
    private WormCombatState_UnknownVirus wormCombatState;
    private TrojanCombatState_UnknownVirus trojanCombatState;
    private RansomwareCombatState_UnknownVirus ransomwareCombatState;
    private DefeatedState_UnknownVirus deadState;
    #endregion

    #region Animation Event Handlers
    // �⺻ ���Ÿ� ���� �ִϸ��̼� �̺�Ʈ

    public void OnMapAttackFinished()
    {
        if (mapAttackState != null)
        {
            mapAttackState.OnAttackFinished();
        }
    }
    
    #endregion

    #region State Setters
    public void SetMapAttackState(MapAttackState_UnknownVirus state)
    {
        mapAttackState = state;
    }

    public void TransformState(TransformState_UnknownVirus state)
    {
        transformState = state;
    }

    public void SetDefeatedState(DefeatedState_UnknownVirus state)
    {
        deadState = state;
    }
    #endregion

    // Runtime
    private float originalAttackTimer;
    private float mapAttackTimer;
    private float formChangeTimer;
    private BossForm currentForm = BossForm.Basic;
    private BossBase currentActiveBoss;
    private bool isTransforming = false;

    public BossForm CurrentForm => currentForm;

    private void Start()
    {
        InitializeComponent(); Debug.Log("�� Component �ʱ�ȭ �Ϸ�");
        InitializeStates(); Debug.Log("�� State �ʱ�ȭ �Ϸ�");
        InitializeFSM(); Debug.Log("�� FSM �ʱ�ȭ �Ϸ�");
    }

    private void Update()
    {
        fsm.Update();
    }

    private void InitializeComponent()
    {
        // �⺻ ����: Player, Animator, NavMeshAgent �� ����
        target = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
        bossStatus.SetMaxHealth(200);
        bossStatus.SetHealth(200f);
        fov = GetComponent<FieldOfView>();
        // ���� ���� �� ��ũ��Ʈ�� ���� ������Ʈ ��ü
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
    }

    private void InitializeFSM()
    {
        // 1) ���� �ν��Ͻ� ����
        var states = CreateStates();

        // 2) FSM ���� (�ʱ� ���´� Intro)
        fsm = new StateMachine<UnknownVirusBoss>(states.introState);

        // 3) ���� ����
        SetupTransitions(states);
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
        // Intro �� Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.introState, s.basicState, () => true));

        // Basic �� MapAttack
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.mapAttackState,
            () => mapAttackTimer >= mapAttackCooldown && UnityEngine.Random.value < mapAttackChance));

        // MapAttack �� Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => s.mapAttackState.IsAnimationFinished()
        ));

        // Basic �� Transform
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => formChangeTimer >= formChangeCooldown && UnityEngine.Random.value < formChangeChance));

        // Transform �� �� ���� ��
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

        // �� ���� �� ���� �� Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.wormCombatState, s.basicState,
            () => currentActiveBoss == null));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.trojanCombatState, s.basicState,
            () => currentActiveBoss == null));
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.ransomwareCombatState, s.basicState,
            () => currentActiveBoss == null));

        // ��� ���¿����� Dead
        List<State<UnknownVirusBoss>> exceptStates = new List<State<UnknownVirusBoss>> { introState };
        fsm.AddGlobalTransition(deadState, () => bossStatus.GetHealth() <= 0, exceptStates);
    }

    #region BasicCombat Helpers
    public void UpdateOriginalAttack()
    {
        originalAttackTimer += Time.deltaTime;
        mapAttackTimer += Time.deltaTime;
        formChangeTimer += Time.deltaTime;

        if (originalAttackTimer >= originalAttackCooldown)
        {
            Instantiate(originalAttackPrefab, transform.position, Quaternion.identity);
            originalAttackTimer = 0f;
        }
    }

    public void TriggerMapAttack()
    {
        Instantiate(mapAttackVFX, transform.position, Quaternion.identity);
        mapAttackTimer = 0f;
    }
    #endregion

    #region Transform Logic
    /// <summary>TransformState ���� ȣ��</summary>
    public void RequestFormChange(BossForm newForm)
    {
        if (isTransforming) return;
        isTransforming = true;
        currentForm = newForm;
        StartCoroutine(TransformRoutine());
    }

    private IEnumerator TransformRoutine()
    {
        transformationVFX?.SetActive(true);
        yield return new WaitForSeconds(transformationTime);
        ApplyForm(currentForm);
        transformationVFX?.SetActive(false);
        isTransforming = false;
    }

    public void ApplyForm(BossForm form)
    {
        // 1) �⺻ �� �����
        gameObject.SetActive(form == BossForm.Basic);

        // 2) ���� �� Ŭ����
        if (currentActiveBoss != null) Destroy(currentActiveBoss.gameObject);

        // 3) �� �� ���� & Ʈ��ŷ
        switch (form)
        {
            case BossForm.Worm:
                currentActiveBoss = Instantiate(wormFormPrefab, transform);
                break;
            case BossForm.Trojan:
                currentActiveBoss = Instantiate(trojanFormPrefab, transform);
                break;
            case BossForm.Ransomware:
                currentActiveBoss = Instantiate(ransomwareFormPrefab, transform);
                break;
            case BossForm.Basic:
            default:
                currentActiveBoss = null;
                gameObject.SetActive(true);
                return;
        }

        // 4) ���� ���� �ݹ� ��� (Defeated �� �ڵ� Basic ����)
       
    }
    #endregion

    public override void TakeDamage(float damage, bool showDamage = true)
    {
        // 1) ��ȯ �߿� ���� ����
        if (isTransforming)
            return;

        // 2) Basic �� �ƴ� ��(���� ����)�� ����
        if (currentForm != BossForm.Basic && currentActiveBoss != null)
        {
            currentActiveBoss.TakeDamage(damage, showDamage);
            return;
        }

        // 3) Basic �� ó��
        // 3-1) ü�� ����
        bossStatus.DecreaseHealth(damage);

        // 3-2) ���� �̺�Ʈ �� UI �˾�
        EventManager.Instance.TriggerMonsterDamagedEvent();
        if (showDamage && UIDamaged != null)
        {
            var popup = Instantiate(
                UIDamaged,
                transform.position + new Vector3(0, UnityEngine.Random.Range(0f, height * 0.5f), 0),
                Quaternion.identity
            ).GetComponent<UIDamage>();
            popup.damage = damage;
        }
       
    }

}
