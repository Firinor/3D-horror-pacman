using Enemy;
using UnityEngine;

public class LevelCheats : MonoBehaviour
{
    public Transform BatteryPool;
    public PlayerController Player;
    public Biha_Enemy Enemy;

    [ContextMenu(nameof(CollectAllBattery))]
    private void CollectAllBattery()
    {
        for(int i = 0; i < BatteryPool.childCount; i++)
        {
            BatteryPool.GetChild(i).position = Player.transform.position + Player.transform.forward * 2;
        }
    }
}