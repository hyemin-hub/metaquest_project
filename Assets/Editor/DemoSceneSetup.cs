#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// B 시연 자동 세팅 도구 (V3 풀스택 게임).
///
/// 메뉴: Tools → B 시연 V3 - 풀스택 게임 자동 세팅
/// 생성: 바닥 + 카메라 + 조명 + GameManager + LevelManager + WeaponInventory + WeaponSpawner + HUD
///        (성벽은 LevelManager가 Level 1으로 자동 생성)
///
/// 게임 흐름:
///   Play → Level 1 시작 (작은 나무 벽)
///   1/2/3 키로 무기 전환
///   클릭/스페이스로 발사
///   70% 부수면 Level 2 (돌벽, 폭탄 늘어남)
///   다시 70% → Level 3 (철벽 + 약점)
///   클리어하면 GAME CLEAR!
/// </summary>
public static class DemoSceneSetup
{
    // 사용자가 설정한 Camera Rig transform 전체 보존용
    static Vector3 savedRigScale = Vector3.one * 32f; // 기본 32 (사용자 설정)
    static Vector3 savedRigPosition = Vector3.zero;
    static Quaternion savedRigRotation = Quaternion.identity;
    static bool hasUserRigTransform = false;

    // 인벤토리 UI 위치 보존
    static Vector3 savedInvPosition = Vector3.zero;
    static Quaternion savedInvRotation = Quaternion.identity;
    static Vector3 savedInvScale = Vector3.one;
    static bool hasUserInvTransform = false;

    [MenuItem("Tools/B 시연 V3 - 풀스택 게임 자동 세팅 (추천)")]
    public static void SetupGame()
    {
        if (!EditorUtility.DisplayDialog(
            "B 시연 V3 풀스택 게임",
            "현재 씬에 전체 게임 시스템을 자동 생성합니다.\n\n" +
            "• 무기 3종 (밤송이/폭탄/가시공) + 1/2/3 키 전환\n" +
            "• 레벨 3개 (나무 → 돌 → 철, 자동 진행)\n" +
            "• 풀 HUD (시간/파괴율/레벨/무기/상태)\n\n계속할까요?",
            "OK", "취소"))
        {
            return;
        }

        CleanupAll();
        SetupGameObjects();

        EditorUtility.DisplayDialog(
            "✅ 풀스택 게임 세팅 완료!",
            "▶ Play\n" +
            "▶ 1/2/3 키 → 무기 전환 (Pebble/Bomb/SpikeBall)\n" +
            "▶ 마우스 클릭 → 클릭한 곳으로 발사\n" +
            "▶ R 키 → 무기 즉시 재스폰\n" +
            "▶ 70% 부수면 자동 다음 레벨!",
            "확인");
    }

    static void CleanupAll()
    {
        string[] names = {
            "Demo_Plane", "Demo_CubeWall", "Demo_Bomb", "Demo_Bomb_Live",
            "Demo_BombSpawner", "Demo_WeaponSpawner", "Demo_GameManager",
            "Demo_LevelManager", "Demo_WeaponInventory", "Demo_HUD", "Demo_Sun"
        };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    static void SetupGameObjects()
    {
        // 바닥 = 두꺼운 무대 (Cube로 변경, 윗면이 Y=0)
        var plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane.name = "Demo_Plane";
        plane.transform.position = new Vector3(0, -0.5f, 0);
        plane.transform.localScale = new Vector3(16, 1f, 10);
        SetColor(plane, new Color(0.42f, 0.46f, 0.52f));

        // 무대 가장자리 라이트 (살짝 톤 다르게)
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "Demo_PlaneTrim";
        trim.transform.SetParent(plane.transform, false);
        trim.transform.localPosition = new Vector3(0, 0.05f, 0);
        trim.transform.localScale = new Vector3(0.97f, 0.02f, 0.97f);
        Object.DestroyImmediate(trim.GetComponent<Collider>());
        SetColor(trim, new Color(0.6f, 0.65f, 0.75f));

        // 인벤토리
        var invGO = new GameObject("Demo_WeaponInventory");
        invGO.AddComponent<WeaponInventory>();

        // 게임 매니저
        var gmGO = new GameObject("Demo_GameManager");
        var gm = gmGO.AddComponent<SimpleGameManager>();
        gm.timeLimit = 60f;
        gm.winRatio = 0.7f;

        // 무기 스포너 — 카메라 바로 1.5m 앞 (1인칭 같은 시점)
        var spawnerGO = new GameObject("Demo_WeaponSpawner");
        var spawner = spawnerGO.AddComponent<WeaponSpawner>();
        spawner.spawnPosition = new Vector3(0.4f, 4.5f, -12.5f);
        spawner.throwForce = 34f;
        spawner.autoRespawnDelay = 1.2f;

        // 레벨 매니저 (성벽 자동 생성 담당)
        var lmGO = new GameObject("Demo_LevelManager");
        var lm = lmGO.AddComponent<LevelManager>();
        lm.winRatio = 0.7f;

        // HUD (Screen Space Overlay)
        var canvasGO = new GameObject("Demo_HUD");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 상단 바
        var topPanel = NewPanel(canvasGO.transform, "TopPanel",
            new Color(0.05f, 0.05f, 0.12f, 0.78f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, 0), new Vector2(0, 160));

        // 타이틀
        TMP_Text title = NewText(topPanel.transform, "TitleText",
            "성벽 부수기 - DEMO", 32, new Color(1f, 0.85f, 0.3f),
            new Vector2(0.5f, 1), new Vector2(0, -25), 600, 50);
        title.alignment = TextAlignmentOptions.Center;

        // 레벨
        TMP_Text levelText = NewText(topPanel.transform, "LevelText",
            "LEVEL 1", 36, new Color(0.95f, 0.7f, 1f),
            new Vector2(0.5f, 1), new Vector2(0, -75), 400, 60);
        levelText.alignment = TextAlignmentOptions.Center;

        // 시간 (왼쪽)
        TMP_Text timeText = NewText(topPanel.transform, "TimeText",
            "TIME 60.0", 38, Color.white,
            new Vector2(0, 1), new Vector2(40, -85), 350, 70);
        timeText.alignment = TextAlignmentOptions.Left;

        // 파괴율 (오른쪽)
        TMP_Text destroyText = NewText(topPanel.transform, "DestroyText",
            "DESTROY 0%", 38, Color.white,
            new Vector2(1, 1), new Vector2(-40, -85), 400, 70);
        destroyText.alignment = TextAlignmentOptions.Right;

        // 하단 바 (현재 무기 + 힌트)
        var bottomPanel = NewPanel(canvasGO.transform, "BottomPanel",
            new Color(0.05f, 0.05f, 0.12f, 0.78f),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
            new Vector2(0, 0), new Vector2(0, 120));

        // 현재 무기 (큰 글자 가운데)
        TMP_Text weaponText = NewText(bottomPanel.transform, "WeaponText",
            "PEBBLE  ∞", 48, new Color(1f, 0.9f, 0.4f),
            new Vector2(0.5f, 1), new Vector2(0, -55), 600, 60);
        weaponText.alignment = TextAlignmentOptions.Center;

        // 힌트
        TMP_Text hintText = NewText(bottomPanel.transform, "HintText",
            "[1] PEBBLE ∞   [2] BOMB 10   [3] SPIKE 3", 22, new Color(0.7f, 0.75f, 0.85f),
            new Vector2(0.5f, 0), new Vector2(0, 15), 800, 40);
        hintText.alignment = TextAlignmentOptions.Center;

        // 상태 메시지 (화면 중앙 큰 글자)
        TMP_Text statusText = NewText(canvasGO.transform, "StatusText",
            "", 120, Color.white,
            new Vector2(0.5f, 0.5f), Vector2.zero, 1200, 200);
        statusText.alignment = TextAlignmentOptions.Center;

        var hud = canvasGO.AddComponent<SimpleGameHUD>();
        hud.timeText = timeText;
        hud.destroyRatioText = destroyText;
        hud.statusText = statusText;
        hud.levelText = levelText;
        hud.weaponText = weaponText;
        hud.weaponHintText = hintText;

        // LevelManager에도 상태 텍스트 연결 (CLEAR 표시용)
        lm.levelText = levelText;
        lm.statusText = statusText;

        // 메인 카메라 — 폭탄 뒤에서 살짝 위에서 내려다보는 3인칭 시점
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 5f, -14f);
            cam.transform.rotation = Quaternion.Euler(18, 0, 0);
            cam.backgroundColor = new Color(0.45f, 0.55f, 0.7f);
            cam.fieldOfView = 60f;
        }

        // 조명
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightGO = new GameObject("Demo_Sun");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }

    static GameObject NewPanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return go;
    }

    static TMP_Text NewText(Transform parent, string name, string content,
        float fontSize, Color color, Vector2 anchor, Vector2 anchoredPos,
        float width, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(width, height);
        return tmp;
    }

    static void SetColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return;
        var mat = new Material(rend.sharedMaterial);
        mat.color = color;
        rend.material = mat;
    }

    static void BuildDebugInputPanel(Vector3 playerPos, Quaternion playerLook)
    {
        // 디버그 입력 표시 패널 - VR에서 항상 보이게
        var canvasGO = new GameObject("Demo_DebugInput");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // 인벤토리 옆에 배치 (왼쪽)
        canvasGO.transform.position = playerPos + playerLook * new Vector3(-20f, 0f, 20f);
        canvasGO.transform.rotation = playerLook;
        canvasGO.transform.localScale = Vector3.one * 0.1f;
        var rect = canvasGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 1000);

        // 배경
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 텍스트
        var textGO = new GameObject("DebugText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "INPUT DEBUG\n초기화중...";
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        var tRect = textGO.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = new Vector2(20, 20);
        tRect.offsetMax = new Vector2(-20, -20);

        // DebugInputDisplay 컴포넌트
        var debug = canvasGO.AddComponent<DebugInputDisplay>();
        debug.displayText = tmp;
    }

    [MenuItem("Tools/B 시연 - mainScene_one에 무기 시스템만 추가 (A 성벽 보존)")]
    public static void SetupWeaponOnly()
    {
        if (!EditorUtility.DisplayDialog(
            "무기 시스템만 추가",
            "현재 씬(mainScene_one 권장)에 무기/HUD/카메라만 추가합니다.\n\n" +
            "• A 팀원의 성벽(TowerHealth) 그대로 활용\n" +
            "• LevelManager + Demo_CubeWall 안 만듦\n" +
            "• 카메라 위치를 시연 좋은 각도로 변경\n\n" +
            "⚠️ MainScene_one에서 작업하면 절대 저장하지 말 것!",
            "OK", "취소"))
        {
            return;
        }

        // Demo_Rig_Parent의 transform 전체 저장 (사용자가 설정한 값 보존)
        var existingRigParent = GameObject.Find("Demo_Rig_Parent");
        if (existingRigParent != null)
        {
            savedRigScale = existingRigParent.transform.localScale;
            savedRigPosition = existingRigParent.transform.position;
            savedRigRotation = existingRigParent.transform.rotation;
            hasUserRigTransform = true;
            Debug.Log($"[SetupWeaponOnly] 기존 Rig 위치/회전/스케일 저장: pos={savedRigPosition}, scale={savedRigScale}");
        }

        // Demo_InventoryUI의 transform 저장 (사용자가 잡은 위치 보존)
        var existingInvUI = GameObject.Find("Demo_InventoryUI");
        if (existingInvUI != null)
        {
            savedInvPosition = existingInvUI.transform.position;
            savedInvRotation = existingInvUI.transform.rotation;
            savedInvScale = existingInvUI.transform.localScale;
            hasUserInvTransform = true;
            Debug.Log($"[SetupWeaponOnly] 기존 인벤토리 위치 저장: {savedInvPosition}");
        }

        // Demo_Rig_Parent 제거 전에 Camera Rig를 root로 빼기 (안 그러면 같이 destroy됨)
        var ovrRigBeforeCleanup = GameObject.Find("[BuildingBlock] Camera Rig");
        if (ovrRigBeforeCleanup != null && ovrRigBeforeCleanup.transform.parent != null
            && ovrRigBeforeCleanup.transform.parent.name == "Demo_Rig_Parent")
        {
            ovrRigBeforeCleanup.transform.SetParent(null);
        }

        // 기존 B 데모 오브젝트만 제거 (A의 성벽은 안 건드림)
        string[] names = {
            "Demo_Plane", "Demo_PlaneTrim", "Demo_CubeWall",
            "Demo_WeaponSpawner", "Demo_WeaponInventory",
            "Demo_GameManager", "Demo_LevelManager", "Demo_HUD", "Demo_Sun",
            "Demo_Rig_Parent", "Demo_InventoryUI", "Demo_InventorySystem", "Demo_DebugInput"
        };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }

        // === 성벽 중심 자동 감지 (Tower 이름 기반 — 가장 정확) ===
        Vector3 towerCenter = Vector3.zero;
        int towerCount = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.StartsWith("Tower") && !go.name.Contains("Health"))
            {
                towerCenter += go.transform.position;
                towerCount++;
            }
        }
        if (towerCount > 0)
        {
            towerCenter /= towerCount;
            Debug.Log($"[SetupWeaponOnly] 성벽 중심: {towerCenter} (Tower* {towerCount}개)");
        }
        else
        {
            Debug.LogWarning("[SetupWeaponOnly] 성벽 못 찾음!");
        }

        // === 플레이어 위치 = 대포 뒤 (성벽 반대 방향) ===
        Vector3 cannonPos = Vector3.zero;
        var cannon = GameObject.Find("Cannon");
        if (cannon != null) cannonPos = cannon.transform.position;
        else cannonPos = towerCenter + new Vector3(0, 0, 15f);

        // 대포 → 성벽 방향
        Vector3 toTower = towerCenter - cannonPos;
        Vector3 forwardDir = new Vector3(toTower.x, 0, toTower.z).normalized;
        Quaternion lookAtTower = Quaternion.LookRotation(forwardDir);

        // 플레이어 = 대포 우측 8m + 살짝 뒤 2m + 위 1.5m
        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized; // 대포 옆 방향
        Vector3 playerPos = cannonPos + rightDir * 8f - forwardDir * 2f + Vector3.up * 1.5f;
        Debug.Log($"[SetupWeaponOnly] 플레이어 위치 (대포 우측 8m): {playerPos}");

        // 무기 인벤토리
        var invGO = new GameObject("Demo_WeaponInventory");
        invGO.transform.position = playerPos;
        invGO.AddComponent<WeaponInventory>();

        // 게임 매니저
        var gmGO = new GameObject("Demo_GameManager");
        gmGO.transform.position = playerPos;
        var gm = gmGO.AddComponent<SimpleGameManager>();
        gm.timeLimit = 180f;
        gm.winRatio = 2f;

        // 무기 스포너 — 플레이어(Cannon) 손 앞쪽
        var spawnerGO = new GameObject("Demo_WeaponSpawner");
        spawnerGO.transform.position = playerPos;
        var spawner = spawnerGO.AddComponent<WeaponSpawner>();
        spawner.gunMode = true; // 강제 켜기
        spawner.spawnOnStart = false; // 자동 spawn 끔
        spawner.spawnPosition = playerPos + lookAtTower * new Vector3(0.4f, 1.5f, 1.5f);
        spawner.throwForce = 38f;
        spawner.weaponScaleMultiplier = 20f; // 사용자 설정값
        spawner.autoRespawnDelay = 1.2f;

        // 에디터 카메라 — 플레이어 위치에서 성벽 방향
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = playerPos + new Vector3(0, 2f, 0);
            cam.transform.rotation = lookAtTower * Quaternion.Euler(10, 0, 0);
            cam.fieldOfView = 65f;
        }

        // OVR Camera Rig — 부모 GameObject로 감싸기
        // ⚠️ 사용자가 한번 위치를 잡은 후로는 자동 세팅이 절대 덮어쓰지 않음
        var ovrRig = GameObject.Find("[BuildingBlock] Camera Rig");
        if (ovrRig != null)
        {
            Transform rigParent = ovrRig.transform.parent;
            if (rigParent == null || rigParent.name != "Demo_Rig_Parent")
            {
                // 새로 만들 때만 위치/스케일 적용
                var parentGO = new GameObject("Demo_Rig_Parent");
                // Camera Rig의 현재 localPosition/Rotation 저장 (사용자가 설정한 것)
                Vector3 rigLocalPos = ovrRig.transform.localPosition;
                Quaternion rigLocalRot = ovrRig.transform.localRotation;

                if (hasUserRigTransform)
                {
                    parentGO.transform.position = savedRigPosition;
                    parentGO.transform.rotation = savedRigRotation;
                    parentGO.transform.localScale = savedRigScale;
                    Debug.Log($"[SetupWeaponOnly] Demo_Rig_Parent 복원: pos={savedRigPosition}, scale={savedRigScale}");
                }
                else
                {
                    parentGO.transform.position = playerPos;
                    parentGO.transform.rotation = lookAtTower;
                    parentGO.transform.localScale = savedRigScale;
                    Debug.Log($"[SetupWeaponOnly] Demo_Rig_Parent 첫 생성: pos={parentGO.transform.position}");
                }
                ovrRig.transform.SetParent(parentGO.transform);
                // ⚠️ 사용자가 Camera Rig에 직접 설정한 위치/회전 그대로 유지 — Vector3.zero로 덮어쓰지 X
                ovrRig.transform.localPosition = rigLocalPos;
                ovrRig.transform.localRotation = rigLocalRot;
            }
            else
            {
                // 이미 있음 — 모든 transform 그대로 유지
                Debug.Log($"[SetupWeaponOnly] Demo_Rig_Parent + Camera Rig 그대로 유지 (사용자 설정 보존)");
            }
        }

        // BuildingBlock Cube — 플레이어 손 앞
        var bbCube = GameObject.Find("[BuildingBlock] Cube");
        if (bbCube != null)
        {
            bbCube.transform.position = playerPos + lookAtTower * new Vector3(0.4f, 1.5f, 1.5f);
            Debug.Log($"[SetupWeaponOnly] BuildingBlock Cube: {bbCube.transform.position}");
        }

        // HUD — 플레이어와 성벽 사이 공중 (둘 다 잘 보이게)
        Vector3 hudPos = Vector3.Lerp(playerPos, towerCenter, 0.5f) + new Vector3(0, 8f, 0);
        BuildSimpleHUD(hudPos, lookAtTower);

        // === 6) Inventory UI 자동 셋업 (D팀원 시스템 활용) ===
        BuildInventoryUI(playerPos, lookAtTower);

        // 디버그 패널 — 시연 끝나면 안 만듦 (필요 시 BuildDebugInputPanel 호출)

        EditorUtility.DisplayDialog(
            "✅ 무기 시스템 추가 완료!",
            "▶ Play\n" +
            "▶ 1/2/3 키 → 무기 전환\n" +
            "▶ 마우스 클릭 → 발사\n" +
            "▶ R 키 → 무기 즉시 재스폰\n\n" +
            "공이 A의 TowerHealth 성벽에 부딪히면 자동 데미지!",
            "확인");
    }

    static void BuildSimpleHUD(Vector3 hudPos, Quaternion playerLook)
    {
        // VR World Space Canvas — 공중에 떠있는 3D 전광판
        var canvasGO = new GameObject("Demo_HUD");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 5;
        canvasGO.AddComponent<GraphicRaycaster>();

        canvasGO.transform.position = hudPos;
        // HUD가 플레이어를 향해 (LookAt 반대방향)
        canvasGO.transform.rotation = playerLook * Quaternion.Euler(0, 180, 0);
        canvasGO.transform.localScale = Vector3.one * 0.02f;
        var rect = canvasGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1000, 600);

        // 배경 패널
        var panelGO = new GameObject("BG");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.12f, 0.85f);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 타이틀
        NewText(canvasGO.transform, "TitleText", "성벽 부수기",
            70, new Color(1f, 0.85f, 0.3f),
            new Vector2(0.5f, 1), new Vector2(0, -70), 900, 100);

        // TIME (왼쪽)
        TMP_Text timeText = NewText(canvasGO.transform, "TimeText", "TIME 180.0",
            90, Color.white,
            new Vector2(0, 0.5f), new Vector2(50, 80), 450, 120);
        timeText.alignment = TextAlignmentOptions.Left;

        // 현재 무기 (오른쪽)
        TMP_Text weaponText = NewText(canvasGO.transform, "WeaponText", "PEBBLE  ∞",
            90, new Color(1f, 0.9f, 0.4f),
            new Vector2(1, 0.5f), new Vector2(-50, 80), 450, 120);
        weaponText.alignment = TextAlignmentOptions.Right;

        // 힌트
        TMP_Text hintText = NewText(canvasGO.transform, "HintText",
            "[1] PEBBLE  [2] BOMB  [3] SPIKE BALL", 40, new Color(0.7f, 0.75f, 0.85f),
            new Vector2(0.5f, 0), new Vector2(0, 50), 900, 70);
        hintText.alignment = TextAlignmentOptions.Center;

        var hud = canvasGO.AddComponent<SimpleGameHUD>();
        hud.timeText = timeText;
        hud.weaponText = weaponText;
        hud.weaponHintText = hintText;
    }

    static void BuildInventoryUI(Vector3 playerPos, Quaternion playerLook)
    {
        // === World Space Canvas — 시야 하단에 떠있는 인벤토리 ===
        var canvasGO = new GameObject("Demo_InventoryUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // VR에서 컨트롤러 ray로 클릭 가능하게 — XR Interaction Toolkit의 TrackedDeviceGraphicRaycaster
        var trdRaycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (trdRaycasterType != null)
        {
            canvasGO.AddComponent(trdRaycasterType);
            Debug.Log("[SetupWeaponOnly] TrackedDeviceGraphicRaycaster 추가 (VR 클릭 가능)");
        }

        // 위치: 사용자가 이전에 잡은 위치 우선 복원, 없으면 자동 위치
        if (hasUserInvTransform)
        {
            canvasGO.transform.position = savedInvPosition;
            canvasGO.transform.rotation = savedInvRotation;
            canvasGO.transform.localScale = savedInvScale;
            Debug.Log($"[SetupWeaponOnly] 인벤토리 위치 복원: {savedInvPosition}");
        }
        else
        {
            // scale 25 환경 — 사용자에게 적당한 거리/크기로 보이게
            Vector3 invPos = playerPos + playerLook * new Vector3(0, -15f, 30f); // 시야 정면 멀리 + 아래
            canvasGO.transform.position = invPos;
            canvasGO.transform.rotation = playerLook;
            canvasGO.transform.localScale = Vector3.one * 0.1f; // 0.008 → 0.1 (크게)
        }
        var canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1400, 700);

        // === 패널 ===
        var panelGO = new GameObject("InventoryPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.10f, 0.08f, 0.92f);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 타이틀
        NewText(panelGO.transform, "Title", "I T E M S",
            70, new Color(0.85f, 0.7f, 0.4f),
            new Vector2(0.5f, 1), new Vector2(0, -50), 600, 100);

        // === 슬롯 6개 — 직접 UI 만들기 ===
        var slotButtons = new Button[6];
        var slotIcons = new Image[6];
        float slotW = 280, slotH = 220, gap = 30;
        float startX = -(slotW + gap);
        float startY = -180;

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3;
            int col = i % 3;
            float x = startX + col * (slotW + gap);
            float y = startY - row * (slotH + gap);

            var slotGO = new GameObject($"Slot_{i + 1}");
            slotGO.transform.SetParent(panelGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.10f, 0.07f, 0.05f, 1f);
            var slotRect = slotGO.GetComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slotRect.pivot = new Vector2(0.5f, 0.5f);
            slotRect.anchoredPosition = new Vector2(x, y);
            slotRect.sizeDelta = new Vector2(slotW, slotH);

            // 아이콘
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(slotGO.transform, false);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            slotButtons[i] = slotGO.AddComponent<Button>();
            slotIcons[i] = iconImg;
        }

        // === CustomInventoryUI 컴포넌트 ===
        var uiSysGO = new GameObject("Demo_InventorySystem");
        uiSysGO.transform.position = playerPos;
        var customInv = uiSysGO.AddComponent<CustomInventoryUI>();
        customInv.panel = panelGO;
        customInv.slotButtons = slotButtons;
        customInv.slotIcons = slotIcons;
        customInv.weaponSpawner = Object.FindFirstObjectByType<WeaponSpawner>();
        customInv.useLeftHand = true;
        customInv.toggleMode = true;

        // === ItemData에서 아이콘/이름/prefab 자동 로드 ===
        var guids = AssetDatabase.FindAssets("t:ItemData");
        var slotDatas = new System.Collections.Generic.List<CustomInventoryUI.SlotData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null) continue;

            slotDatas.Add(new CustomInventoryUI.SlotData
            {
                itemName = item.itemName,
                icon = item.icon,
                prefab = item.projectilePrefab
            });
            if (slotDatas.Count >= 6) break;
        }

        // 부족하면 빈 슬롯으로 채움
        while (slotDatas.Count < 6)
        {
            slotDatas.Add(new CustomInventoryUI.SlotData { itemName = "(빈 슬롯)" });
        }
        customInv.slots = slotDatas.ToArray();

        Debug.Log($"[SetupWeaponOnly] CustomInventoryUI 생성 완료 ({slotDatas.Count}개 슬롯, Y버튼/I키 토글)");
    }

    [MenuItem("Tools/B 시연 - 6개 무기 Prefab 자동 생성 + ItemData 자동 할당")]
    public static void AutoCreateWeaponPrefabs()
    {
        // 1. 베이스 prefab (GrabBall_Base)
        GameObject baseBall = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/GrabBall_Base.prefab");
        if (baseBall == null)
        {
            EditorUtility.DisplayDialog("실패", "Assets/Prefabs/Projectiles/GrabBall_Base.prefab 못 찾음", "OK");
            return;
        }

        // 2. UI 폴더의 모델 파일 (확장자 포함)
        string[] modelPaths = {
            "Assets/UI/Eight ball.glb",
            "Assets/UI/Spiky Ball.glb",
            "Assets/UI/Potion Bottle - Game Asset (1).glb",
            "Assets/UI/MDR.glb",
            "Assets/UI/Assault Rifle.glb",
            "Assets/UI/Pistol_5.fbx",
        };
        string[] weaponNames = {
            "EightBall", "SpikyBall", "Potion", "MDR", "AssaultRifle", "Pistol"
        };

        // 3. ItemData 6개 찾기
        var itemGuids = AssetDatabase.FindAssets("t:ItemData");
        var items = new System.Collections.Generic.List<ItemData>();
        foreach (var g in itemGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data != null) items.Add(data);
        }

        int created = 0;
        for (int i = 0; i < 6; i++)
        {
            // 4. GrabBall_Base 인스턴스화
            var newInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseBall);
            newInstance.name = $"GrabBall_{weaponNames[i]}";

            // 5. 원본 큐브 메시 비활성화 (있으면)
            var cubeRenderer = newInstance.GetComponent<MeshRenderer>();
            if (cubeRenderer != null) cubeRenderer.enabled = false;

            // 6. 모델 자식으로 추가
            string mPath = i < modelPaths.Length ? modelPaths[i] : null;
            if (mPath != null)
            {
                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(mPath);
                if (modelAsset != null)
                {
                    var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, newInstance.transform);
                    modelInstance.transform.localPosition = Vector3.zero;
                    modelInstance.transform.localRotation = Quaternion.identity;
                    modelInstance.transform.localScale = Vector3.one * 0.5f;
                }
                else
                {
                    Debug.LogWarning($"[AutoPrefab] 모델 못 찾음: {mPath}");
                }
            }

            // 7. 새 prefab으로 저장
            string newPrefabPath = $"Assets/Prefabs/Projectiles/GrabBall_{weaponNames[i]}.prefab";
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(newInstance, newPrefabPath);
            Object.DestroyImmediate(newInstance);

            // 8. ItemData에 할당
            if (i < items.Count && savedPrefab != null)
            {
                items[i].projectilePrefab = savedPrefab;
                EditorUtility.SetDirty(items[i]);
                created++;
                Debug.Log($"[AutoPrefab] {items[i].itemName} → {savedPrefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "✅ 완료",
            $"{created}개 무기 prefab 생성 + ItemData 자동 할당 완료!\n\n" +
            "이제 Tools → 자동 생성 모두 제거 → 다시 추가하면 적용됨",
            "확인");
    }

    [MenuItem("Tools/B 시연 - 자동 생성한 오브젝트 모두 제거")]
    public static void CleanupMenu()
    {
        if (!EditorUtility.DisplayDialog(
            "데모 오브젝트 제거",
            "Demo_ 로 시작하는 오브젝트를 모두 제거합니다.",
            "OK", "취소"))
        {
            return;
        }
        CleanupAll();
        EditorUtility.DisplayDialog("완료", "모든 Demo_ 오브젝트 제거됨", "OK");
    }
}
#endif
