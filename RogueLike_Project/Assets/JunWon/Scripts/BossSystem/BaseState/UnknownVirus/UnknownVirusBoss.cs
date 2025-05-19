using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEngine.UI.GridLayoutGroup;

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
    private float lastMapAttackTime = 15f;
    [SerializeField] private GameObject mapAttackVFX;
    [SerializeField] private float mapAttackCooldown = 15f;
    [Range(0, 1)][SerializeField] private float mapAttackChance = 0.8f;

    [Header("Form Change")]
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private float transformationTime = 3f;
    [SerializeField] private float formChangeCooldown = 30f;
    [Range(0, 1)][SerializeField] private float formChangeChance = 0.3f;
    #endregion

    #region Components
    [Header("Basic Components")]
    [SerializeField] private AbilityManager abilityManager;
    #endregion

    #region Status & State
    // FSM & States
    [Header("State Machine")]
    [SerializeField] private StateMachine<UnknownVirusBoss> fsm;

    [Header("State")]
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

    #region Public Properties
    public Transform Player => target;
    public NavMeshAgent NmAgent => nmAgent;
    public Animator Animator => anim;
    public BossStatus MonsterStatus => bossStatus;
    public FieldOfView FOV => fov;
    public AbilityManager AbilityManager => abilityManager;
    #endregion

    #region MapAttack
    [Header("Map Attack Settings")]
    [SerializeField] private Collider[] hitColliders;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int attackAreaSize = 5; // ���� ���� ũ�� (5x5)
    [SerializeField] private float tileSearchInterval = 0.1f; // Ÿ�� �˻� ����
    [SerializeField] private float shockwavePower = 30f; // ����� ����
    [SerializeField] private float searchCooldown = 3f;
    [SerializeField] private LayerMask playerLayer;

    // �� ���ݿ� Ÿ��
    #endregion




    // Runtime
    private BossForm currentForm = BossForm.Basic;
    private BossBase currentActiveBoss;
    private bool isTransforming = false;

    public BossForm CurrentForm => currentForm;

    private void Start()
    {
        InitializeComponent(); Debug.Log("�� Component �ʱ�ȭ �Ϸ�");
        InitializeAbility(); Debug.Log("�� ��� �ʱ�ȭ �Ϸ�");
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
        tileManager = FindObjectOfType<TileManager>();
        target = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
        nmAgent = GetComponent<NavMeshAgent>();
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
            () => abilityManager.GetAbilityRemainingCooldown("MapAttack") == 0 && UnityEngine.Random.value < mapAttackChance));

        // MapAttack �� Basic
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.mapAttackState, s.basicState,
            () => s.mapAttackState.IsAnimationFinished()
        ));

        // Basic �� Transform
        fsm.AddTransition(new Transition<UnknownVirusBoss>(
            s.basicState, s.transformState,
            () => UnityEngine.Random.value < formChangeChance));

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

    #region AttackAbility Helpers
    private void InitializeAbility()
    {
        // �߾� ���Ͽ��� ����� �ɷ� Ȱ��ȭ
        abilityManager.SetAbilityActive("MapAttack");
        abilityManager.SetMaxCoolTime("MapAttack");

        // �ٸ� ��� �ɷ� ��Ȱ��ȭ
        //owner.AbilityManager.SetAbilityInactive();
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


    #region MapAttack Func
    // �� ���� Ʈ���� �޼���
    public void TriggerMapAttack()
    {
        try
        {
            lastMapAttackTime = Time.time;

            if (tileManager == null)
            {
                Debug.LogError("Cannot execute map attack: TileManager is null");
                mapAttackState?.OnAttackFinished();
                return;
            }

            // �˻� �˰��� ����
            int searchMethod = UnityEngine.Random.Range(0, 3);
            StartCoroutine(ExecuteMapAttack(searchMethod));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TriggerMapAttack error: {e.Message}\n{e.StackTrace}");
            mapAttackState?.OnAttackFinished();
        }
    }

    private IEnumerator ExecuteMapAttack(int searchMethod)
    {
        // �÷��̾� �ֺ� ��ǥ ���
        Vector3 playerPos = target.position;

        // ���� ��ǥ�� TileManager�� �׸��� ��ǥ�� ��ȯ
        int centerX = Mathf.RoundToInt(playerPos.x / 2);
        int centerZ = Mathf.RoundToInt(playerPos.z / 2);

        Debug.Log($"Starting map attack at grid position: [{centerX}, {centerZ}]");

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

        Debug.Log($"Linear search area: [{minX},{minZ}] to [{maxX},{maxZ}], target at: [{targetX},{targetZ}]");

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

                    // ������ ������ ����
                    VirusLaser virusLaser = laser.GetComponent<VirusLaser>();
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

                    // ������ ����
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

        Debug.Log($"Binary search area: [{leftX},{topZ}] to [{rightX},{bottomZ}], target at: [{targetX},{targetZ}]");

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
                            VirusLaser virusLaser = laser.GetComponent<VirusLaser>();
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

                Vector3 targetPos = new Vector3(midX * 2, 0, midZ * 2);

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

        // Ÿ���� ã�� ���� ��� (�̷� ���� ����� ��)
        HighlightTile(targetX, targetZ, Color.green);

        Vector3 finalPos = new Vector3(targetX * 2, 0, targetZ * 2);
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

        Debug.Log($"Random search area: [{minX},{minZ}] to [{maxX},{maxZ}], target at: [{targetX},{targetZ}]");

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

        Debug.Log("Apply Damage");
        // ������ ����
        hitColliders = Physics.OverlapSphere(centerPosition, damageRadius, playerLayer);
        foreach (var collider in hitColliders)
        {
            if (collider == null) continue;
            Debug.Log(collider + "is hit");

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
