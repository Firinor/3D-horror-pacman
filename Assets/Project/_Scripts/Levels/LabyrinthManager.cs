using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabyrinthManager : MonoBehaviour
{
    public PlayerController Player;
    public Transform BatteryPool;

    public string Prefix;
    
    public Toggle SurviveToggle;
    public Toggle BatteryToggle;
    
    public TextMeshProUGUI BatteryCountText;
    
    private void Awake()
    {
        Player.OnBatteryPick += RefreshBatteryCountText;
        RefreshBatteryCountText();
    }

    private void RefreshBatteryCountText()
    {
        BatteryCountText.text = Prefix + (BatteryPool.childCount-1);
    }


    private void OnDestroy()
    {
        Player.OnBatteryPick -= RefreshBatteryCountText;
    }
}
