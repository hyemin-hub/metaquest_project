using UnityEngine;
using TMPro;

public class TowerHpUI : MonoBehaviour
{
    public TowerHealth tower;
    public TMP_Text hpText;

    void Update()
    {
        if (tower.hp > 0)
        {
            hpText.text =
                "HP : " +
                tower.hp.ToString("F0");
        }
        else
        {
            hpText.text = "Destroyed";
        }
    }
}
