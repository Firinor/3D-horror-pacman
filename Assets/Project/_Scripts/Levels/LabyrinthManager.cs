using System.Collections;
using System.Linq;
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
    public Toggle KeyToggle;
    public Toggle PortalToggle;
    
    public TextMeshProUGUI BatteryCountText;
    public TextMeshProUGUI MessageText;

    public Transform[] spawnPoints;

    public GameObject KeyPrefab;
    public GameObject PortalPrefab;
    
    private void Awake()
    {
        Player.OnBatteryPick += RefreshBatteryCountText;
        Player.OnKeyPick += SpawnPortal;
        RefreshBatteryCountText();
    }

    private void SpawnPortal()
    {
        KeyToggle.isOn = true;
        PortalToggle.gameObject.SetActive(true);
        StartCoroutine(ShowPortalMessage());
        Vector3 spawnPoint = GetRandomSpawnPoitn();
            
        Instantiate(PortalPrefab, spawnPoint, Quaternion.identity, BatteryPool);
    }

    private void RefreshBatteryCountText()
    {
        BatteryCountText.text = Prefix + (BatteryPool.childCount-1);

        if (BatteryPool.childCount <= 1)
        {
            BatteryToggle.isOn = true;
            KeyToggle.gameObject.SetActive(true);
            StartCoroutine(ShowKeyMessage());
            Vector3 spawnPoint = GetRandomSpawnPoitn();
            
            Instantiate(KeyPrefab, spawnPoint, Quaternion.identity, BatteryPool);
        }
    }

    private Vector3 GetRandomSpawnPoitn()
    {
        var sortedPoints = spawnPoints
            .Where(p => p != null)
            .OrderByDescending(p => Vector3.Distance(Player.transform.position, p.position))
            .Take(5)
            .ToList();

        Transform randomPoint = sortedPoints.GetRandom();

        return randomPoint.position;
    }

    private IEnumerator ShowKeyMessage()
    {
        MessageText.text = "Key spawned!";
        MessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        MessageText.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowPortalMessage()
    {
        MessageText.text = "Door is open!";
        MessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        MessageText.gameObject.SetActive(false);
    }


    private void OnDestroy()
    {
        Player.OnBatteryPick -= RefreshBatteryCountText;
        Player.OnKeyPick -= SpawnPortal;
    }
}
