using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 풀스택 HUD — 시간 / 파괴율 / 레벨 / 현재 무기 / 남은 횟수 / 상태.
/// Screen Space Overlay로 화면 잘리지 않게 표시.
/// </summary>
public class SimpleGameHUD : MonoBehaviour
{
    [Header("연결할 텍스트 (TMP)")]
    public TMP_Text timeText;
    public TMP_Text destroyRatioText;
    public TMP_Text statusText;
    public TMP_Text levelText;
    public TMP_Text weaponText;
    public TMP_Text weaponHintText;

    void Update()
    {
        var gm = SimpleGameManager.Instance;
        if (gm != null)
        {
            if (timeText != null)
                timeText.text = $"TIME  {Mathf.Max(0f, gm.timeRemaining):00.0}";

            if (destroyRatioText != null)
                destroyRatioText.text = $"DESTROY  {gm.GetDestroyRatio():P0}";

            if (statusText != null && !gm.gameEnded)
            {
                // 레벨 매니저에서 SUCCESS/CLEAR 텍스트 설정함. 빈 상태만 유지
                if (statusText.text == "" || statusText.text.Contains("FAIL"))
                {
                    if (gm.gameEnded && gm.GetDestroyRatio() < gm.winRatio)
                    {
                        statusText.text = "<color=#e25656>FAILED</color>";
                    }
                }
            }
        }

        var lm = LevelManager.Instance;
        if (lm != null && levelText != null)
        {
            levelText.text = $"LEVEL {lm.currentLevel}";
        }

        var inv = WeaponInventory.Instance;
        if (inv != null && weaponText != null)
        {
            string weaponName = "";
            int count = inv.GetCount(inv.currentWeapon);
            string countStr = count == -1 ? "∞" : count.ToString();

            switch (inv.currentWeapon)
            {
                case WeaponType.Pebble: weaponName = "PEBBLE"; break;
                case WeaponType.Bomb: weaponName = "BOMB"; break;
                case WeaponType.SpikeBall: weaponName = "SPIKE BALL"; break;
            }
            weaponText.text = $"{weaponName}  {countStr}";
        }

        if (inv != null && weaponHintText != null)
        {
            int p = inv.pebbleCount;
            int b = inv.bombCount;
            int s = inv.spikeBallCount;
            string pStr = p == -1 ? "∞" : p.ToString();
            weaponHintText.text = $"[1] PEBBLE {pStr}   [2] BOMB {b}   [3] SPIKE {s}";
        }
    }
}
