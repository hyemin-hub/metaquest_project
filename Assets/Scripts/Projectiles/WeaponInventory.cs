using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 무기 인벤토리 — 종류별 보유 수량 관리, 무기 전환.
/// 1, 2, 3 키로 전환.
/// </summary>
public class WeaponInventory : MonoBehaviour
{
    [Header("보유 수량 (-1 = 무제한)")]
    public int pebbleCount = -1;
    public int bombCount = 10;
    public int spikeBallCount = 3;

    [Header("현재 선택된 무기")]
    public WeaponType currentWeapon = WeaponType.Pebble;

    [Header("키 전환 활성화")]
    public bool allowKeyboardSwitch = true;

    public static WeaponInventory Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 키보드 (에디터 테스트용)
        if (allowKeyboardSwitch)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(WeaponType.Pebble);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(WeaponType.Bomb);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(WeaponType.SpikeBall);
        }

        // OpenXR 호환 — UnityEngine.XR.InputDevices
        // (이 시스템은 CustomInventoryUI랑 충돌 — 이 안에서는 비활성)
    }

    public void CycleWeapon(int direction)
    {
        int cur = (int)currentWeapon;
        int max = System.Enum.GetValues(typeof(WeaponType)).Length;
        int next = ((cur + direction) % max + max) % max;
        SelectWeapon((WeaponType)next);
    }

    public void SelectWeapon(WeaponType w)
    {
        currentWeapon = w;
        Debug.Log($"[Inventory] 무기 전환: {w} (남은: {GetCount(w)})");
    }

    public int GetCount(WeaponType w)
    {
        switch (w)
        {
            case WeaponType.Pebble: return pebbleCount;
            case WeaponType.Bomb: return bombCount;
            case WeaponType.SpikeBall: return spikeBallCount;
        }
        return 0;
    }

    public bool CanUse(WeaponType w)
    {
        int c = GetCount(w);
        return c == -1 || c > 0;
    }

    /// <summary>
    /// 무기 사용. 횟수 차감.
    /// </summary>
    public bool TryUseCurrent()
    {
        if (!CanUse(currentWeapon)) return false;

        switch (currentWeapon)
        {
            case WeaponType.Pebble:
                // 무제한 (-1)이면 차감 안 함
                if (pebbleCount > 0) pebbleCount--;
                break;
            case WeaponType.Bomb:
                bombCount--;
                break;
            case WeaponType.SpikeBall:
                spikeBallCount--;
                break;
        }
        return true;
    }

    /// <summary>레벨 전환 시 무기 충전.</summary>
    public void Refill(int pebble, int bomb, int spike)
    {
        pebbleCount = pebble;
        bombCount = bomb;
        spikeBallCount = spike;
    }
}
