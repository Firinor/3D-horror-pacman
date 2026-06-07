using System.Collections;
using System.Linq;
using Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabyrinthManager : MonoBehaviour
{
    public PlayerController Player;
    public Transform BatteryPool;
    public Biha_Enemy Enemy;

    public string Prefix;
    
    public Toggle SurviveToggle;
    public Toggle BatteryToggle;
    public Toggle KeyToggle;
    public Toggle PortalToggle;
    
    public TextMeshProUGUI BatteryCountText;
    public TextMeshProUGUI MessageText;
    public TextMeshProUGUI EndScreenText;
    public GameObject EndScreen;
    public float EndScreenTimer = 4f;
    
    public Transform[] spawnPoints;

    public GameObject KeyPrefab;
    public GameObject PortalPrefab;
    
    private int batteryCount;
    
    private void Awake()
    {
        batteryCount = BatteryPool.childCount;
        
        Player.OnBatteryPick += RefreshBatteryCount;
        Player.OnKeyPick += SpawnPortal;
        Enemy.OnPlayerCach += ShowLoseScreen;
        RefreshBatteryCountText();
    }

    private void SpawnPortal()
    {
        KeyToggle.isOn = true;
        PortalToggle.gameObject.SetActive(true);
        StartCoroutine(ShowPortalMessage());
        Vector3 spawnPoint = GetRandomSpawnPoitn();
            
        var portal = Instantiate(PortalPrefab, spawnPoint, Quaternion.identity, BatteryPool);
        portal.GetComponent<LookAt>()
            .Initialize(Player.transform);
        portal.GetComponent<WinTrigger>()
            .OnPlayerEnter += ShowWinScreen;
    }

    private void ShowWinScreen()
    {
        Enemy.enabled = false;
        PortalToggle.isOn = true;
        Player.ToParalyze();
        EndScreenText.text = "You WIN!";
        StartCoroutine(DelayBeforeEndScreen());
    }
    private void ShowLoseScreen()
    {
        EndScreenText.text = "You LOSE!";
        StartCoroutine(DelayBeforeEndScreen());
    }

    private IEnumerator DelayBeforeEndScreen()
    {
        yield return new WaitForSeconds(EndScreenTimer);
        EndScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void RefreshBatteryCount()
    {
        batteryCount--;
        
        RefreshBatteryCountText();
        
        if (batteryCount > 0) 
            return;
        
        BatteryCountText.gameObject.SetActive(false);
        BatteryToggle.isOn = true;
        KeyToggle.gameObject.SetActive(true);
        StartCoroutine(ShowKeyMessage());
        Vector3 spawnPoint = GetRandomSpawnPoitn();
            
        Instantiate(KeyPrefab, spawnPoint + KeyPrefab.transform.position, Quaternion.identity, BatteryPool);
    }

    private void RefreshBatteryCountText()
    {
        BatteryCountText.text = Prefix + batteryCount;
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
        Enemy.OnPlayerCach -= ShowLoseScreen;
    }
}
