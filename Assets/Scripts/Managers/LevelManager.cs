using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 레벨 진행 관리.
/// Level 1: 약한 나무벽 (HP 12, 작은 성)
/// Level 2: 중간 돌벽 (HP 30, 중간 성)
/// Level 3: 강한 철벽 (HP 60, 큰 성 + 약점)
///
/// 각 레벨 클리어 = 70% 부수기 → 다음 레벨로 자동 전환
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("레벨 진행 상태")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    [Header("레벨 클리어 조건")]
    public float winRatio = 0.7f;

    [Header("레벨별 시간 (초)")]
    public float[] levelTimes = { 60f, 75f, 90f };

    [Header("레벨별 무기 보유")]
    public int[] levelBombCounts = { 5, 10, 15 };
    public int[] levelSpikeCounts = { 0, 3, 5 };

    [Header("레벨 전환 효과")]
    public float transitionDelay = 2.5f;

    [Header("UI 참조 (선택)")]
    public TMP_Text levelText;
    public TMP_Text statusText;

    public static LevelManager Instance { get; private set; }

    private bool transitioning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartLevel(1);
    }

    public void StartLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        transitioning = false;

        // 새 레벨 블럭들이 다시 고정되도록
        SimpleHealthBlock.ResetFreezeState();

        // 무기 인벤토리 갱신
        if (WeaponInventory.Instance != null)
        {
            int pebble = -1; // 무제한
            int bomb = currentLevel - 1 < levelBombCounts.Length ? levelBombCounts[currentLevel - 1] : 10;
            int spike = currentLevel - 1 < levelSpikeCounts.Length ? levelSpikeCounts[currentLevel - 1] : 3;
            WeaponInventory.Instance.Refill(pebble, bomb, spike);
            WeaponInventory.Instance.SelectWeapon(WeaponType.Pebble);
        }

        // 게임 매니저 갱신
        if (SimpleGameManager.Instance != null)
        {
            float time = currentLevel - 1 < levelTimes.Length ? levelTimes[currentLevel - 1] : 60f;
            SimpleGameManager.Instance.timeLimit = time;
            SimpleGameManager.Instance.winRatio = winRatio;
            SimpleGameManager.Instance.timeRemaining = time;
            SimpleGameManager.Instance.gameEnded = false;
            SimpleGameManager.Instance.destroyedBlocks = 0;
        }

        // 기존 성벽 제거 + 새 성벽 만들기
        BuildLevelWall(currentLevel);

        // GameManager 블록 수 다시 카운트
        if (SimpleGameManager.Instance != null)
        {
            SimpleGameManager.Instance.totalBlocks = FindObjectsByType<SimpleHealthBlock>(FindObjectsSortMode.None).Length;
        }

        if (levelText != null) levelText.text = $"LEVEL {currentLevel}";

        Debug.Log($"[LevelManager] Level {currentLevel} 시작!");
    }

    public void OnLevelComplete()
    {
        if (transitioning) return;
        transitioning = true;

        if (currentLevel >= maxLevel)
        {
            // 최종 클리어
            if (statusText != null) statusText.text = "<color=#ffd23b>GAME CLEAR!</color>";
            Debug.Log("[LevelManager] 🏆 전체 클리어!");
            return;
        }

        if (statusText != null) statusText.text = $"<color=#46e26a>LEVEL {currentLevel} CLEAR!</color>";
        StartCoroutine(NextLevelRoutine());
    }

    IEnumerator NextLevelRoutine()
    {
        yield return new WaitForSeconds(transitionDelay);
        if (statusText != null) statusText.text = "";
        StartLevel(currentLevel + 1);
    }

    void BuildLevelWall(int level)
    {
        // 기존 성벽 제거
        var existing = GameObject.Find("Demo_CubeWall");
        if (existing != null) Destroy(existing);

        // 레벨별 다른 디자인
        var wallParent = new GameObject("Demo_CubeWall");

        switch (level)
        {
            case 1: BuildLevel1Wall(wallParent); break;
            case 2: BuildLevel2Wall(wallParent); break;
            case 3: BuildLevel3Wall(wallParent); break;
        }
    }

    // === Level 1: 작은 나무 미니 벽 (한 방에 부서지게 약함) ===
    void BuildLevel1Wall(GameObject parent)
    {
        Color woodLight = new Color(0.75f, 0.55f, 0.35f);
        Color woodDark = new Color(0.5f, 0.35f, 0.2f);

        // 작은 벽 (3 wide x 2 tall)
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                brick.name = $"L1_Brick_{x}_{y}";
                brick.transform.localScale = new Vector3(1.3f, 1.3f, 0.7f);
                brick.transform.position = new Vector3((x - 1f) * 1.3f, 0.65f + y * 1.3f, 0);
                brick.transform.SetParent(parent.transform);

                var rb = brick.AddComponent<Rigidbody>();
                rb.mass = 1.2f;

                var hb = brick.AddComponent<SimpleHealthBlock>();
                hb.maxHP = 6f; // 펫블 1방(10데미지)에 부서짐

                SetColor(brick, (x + y) % 2 == 0 ? woodLight : woodDark);
            }
        }

        // 작은 톱니 3개
        for (int i = 0; i < 3; i++)
        {
            var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = $"L1_Tooth_{i}";
            tooth.transform.localScale = new Vector3(0.5f, 0.6f, 0.7f);
            tooth.transform.position = new Vector3((i - 1f) * 1.3f, 3f, 0);
            tooth.transform.SetParent(parent.transform);

            var rb = tooth.AddComponent<Rigidbody>();
            rb.mass = 0.8f;
            var hb = tooth.AddComponent<SimpleHealthBlock>();
            hb.maxHP = 4f; // 펫블 1방에 부서짐
            SetColor(tooth, woodLight);
        }
    }

    // === Level 2: 중간 크기 돌벽 + 탑 + 깃발 (이전 디자인) ===
    void BuildLevel2Wall(GameObject parent)
    {
        Color stoneLight = new Color(0.78f, 0.72f, 0.62f);
        Color stoneDark = new Color(0.55f, 0.50f, 0.43f);
        Color roofColor = new Color(0.35f, 0.20f, 0.18f);
        Color doorColor = new Color(0.30f, 0.18f, 0.10f);

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (y == 0 && (x == 1 || x == 2)) continue;
                var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                brick.name = $"L2_Brick_{x}_{y}";
                brick.transform.localScale = new Vector3(1.4f, 1.4f, 0.8f);
                brick.transform.position = new Vector3((x - 1.5f) * 1.4f, 0.7f + y * 1.4f, 0);
                brick.transform.SetParent(parent.transform);

                var rb = brick.AddComponent<Rigidbody>();
                rb.mass = 2f;
                var hb = brick.AddComponent<SimpleHealthBlock>();
                hb.maxHP = 30f;
                SetColor(brick, (x + y) % 2 == 0 ? stoneLight : stoneDark);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = $"L2_Tooth_{i}";
            tooth.transform.localScale = new Vector3(0.6f, 0.7f, 0.8f);
            tooth.transform.position = new Vector3((i - 2f) * 1.1f, 3.25f, 0);
            tooth.transform.SetParent(parent.transform);
            var rb = tooth.AddComponent<Rigidbody>();
            rb.mass = 1f;
            var hb = tooth.AddComponent<SimpleHealthBlock>();
            hb.maxHP = 20f;
            SetColor(tooth, stoneLight);
        }

        for (int side = -1; side <= 1; side += 2)
        {
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = $"L2_Tower_{side}";
            tower.transform.localScale = new Vector3(0.9f, 2.2f, 0.9f);
            tower.transform.position = new Vector3(side * 3.5f, 2.2f, 0);
            tower.transform.SetParent(parent.transform);
            var rb = tower.AddComponent<Rigidbody>();
            rb.mass = 3f;
            var hb = tower.AddComponent<SimpleHealthBlock>();
            hb.maxHP = 40f;
            SetColor(tower, stoneDark);

            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = $"L2_Spire_{side}";
            spire.transform.localScale = new Vector3(0.55f, 0.6f, 0.55f);
            spire.transform.position = new Vector3(side * 3.5f, 5f, 0);
            spire.transform.SetParent(parent.transform);
            var srb = spire.AddComponent<Rigidbody>();
            srb.mass = 1f;
            var shb = spire.AddComponent<SimpleHealthBlock>();
            shb.maxHP = 15f;
            SetColor(spire, roofColor);
        }

        var gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate.name = "L2_Gate";
        gate.transform.localScale = new Vector3(2.6f, 1.4f, 0.7f);
        gate.transform.position = new Vector3(0, 0.7f, 0);
        gate.transform.SetParent(parent.transform);
        var grb = gate.AddComponent<Rigidbody>();
        grb.mass = 2.5f;
        var ghb = gate.AddComponent<SimpleHealthBlock>();
        ghb.maxHP = 35f;
        SetColor(gate, doorColor);
    }

    // === Level 3: 큰 철벽 (철색, 더 많은 블록, 약점 표시) ===
    void BuildLevel3Wall(GameObject parent)
    {
        Color ironLight = new Color(0.55f, 0.6f, 0.7f);
        Color ironDark = new Color(0.3f, 0.35f, 0.45f);
        Color weakPoint = new Color(0.9f, 0.2f, 0.15f); // 약점 빨강
        Color crown = new Color(0.7f, 0.55f, 0.2f);     // 황금 왕관 장식

        // 큰 벽 (5 wide x 3 tall)
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (y == 0 && (x == 2)) continue; // 성문 자리

                var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                brick.name = $"L3_Brick_{x}_{y}";
                brick.transform.localScale = new Vector3(1.4f, 1.4f, 0.9f);
                brick.transform.position = new Vector3((x - 2f) * 1.4f, 0.7f + y * 1.4f, 0);
                brick.transform.SetParent(parent.transform);

                var rb = brick.AddComponent<Rigidbody>();
                rb.mass = 3f;
                var hb = brick.AddComponent<SimpleHealthBlock>();

                // 가운데 1개는 약점 (낮은 HP, 빨강)
                bool isWeak = (x == 2 && y == 1);
                hb.maxHP = isWeak ? 20f : 60f;
                SetColor(brick, isWeak ? weakPoint : ((x + y) % 2 == 0 ? ironLight : ironDark));
                if (isWeak) brick.tag = "Untagged";
            }
        }

        // 톱니 (7개)
        for (int i = 0; i < 7; i++)
        {
            var tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = $"L3_Tooth_{i}";
            tooth.transform.localScale = new Vector3(0.55f, 0.8f, 0.9f);
            tooth.transform.position = new Vector3((i - 3f) * 1f, 4.95f, 0);
            tooth.transform.SetParent(parent.transform);
            var rb = tooth.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            var hb = tooth.AddComponent<SimpleHealthBlock>();
            hb.maxHP = 30f;
            SetColor(tooth, ironLight);
        }

        // 양 옆 큰 탑
        for (int side = -1; side <= 1; side += 2)
        {
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = $"L3_Tower_{side}";
            tower.transform.localScale = new Vector3(1.1f, 3f, 1.1f);
            tower.transform.position = new Vector3(side * 4.2f, 3f, 0);
            tower.transform.SetParent(parent.transform);
            var rb = tower.AddComponent<Rigidbody>();
            rb.mass = 4f;
            var hb = tower.AddComponent<SimpleHealthBlock>();
            hb.maxHP = 80f;
            SetColor(tower, ironDark);

            // 황금 왕관 (탑 꼭대기)
            var topCrown = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topCrown.name = $"L3_Crown_{side}";
            topCrown.transform.localScale = new Vector3(1.3f, 0.3f, 1.3f);
            topCrown.transform.position = new Vector3(side * 4.2f, 6.2f, 0);
            topCrown.transform.SetParent(parent.transform);
            var crb = topCrown.AddComponent<Rigidbody>();
            crb.mass = 1.5f;
            var chb = topCrown.AddComponent<SimpleHealthBlock>();
            chb.maxHP = 25f;
            SetColor(topCrown, crown);
        }

        // 큰 철문
        var gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate.name = "L3_Gate";
        gate.transform.localScale = new Vector3(1.4f, 1.4f, 0.9f);
        gate.transform.position = new Vector3(0, 0.7f, 0);
        gate.transform.SetParent(parent.transform);
        var ggrb = gate.AddComponent<Rigidbody>();
        ggrb.mass = 3.5f;
        var gghb = gate.AddComponent<SimpleHealthBlock>();
        gghb.maxHP = 60f;
        SetColor(gate, new Color(0.2f, 0.25f, 0.35f));
    }

    void SetColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return;
        var mat = new Material(rend.sharedMaterial);
        mat.color = color;
        rend.material = mat;
    }
}
