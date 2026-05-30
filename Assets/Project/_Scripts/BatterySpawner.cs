using System.Collections.Generic;
using UnityEngine;

public class BatterySpawner : MonoBehaviour
{
    public GameObject batteryPrefab;

    [Header("Spawn Settings")]
    public float spawnHeight = 0.3f;
    public float step = 2.0f;

    void Start()
    {
        if (batteryPrefab == null) return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        
        float startX = Mathf.Round(player.transform.position.x / step) * step;
        float startZ = Mathf.Round(player.transform.position.z / step) * step;

        Vector3 startNode = new Vector3(startX, spawnHeight, startZ);

        Queue<Vector3> queue = new Queue<Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        int spawnedCount = 0;
        
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        
        while (queue.Count > 0 && spawnedCount < 5000)
        {
            Vector3 currentPos = queue.Dequeue();
            
            Vector3 checkPos = new Vector3(currentPos.x, 1.5f, currentPos.z);
            Collider[] colliders = Physics.OverlapSphere(checkPos, 0.4f);

            bool isWall = false;
            foreach (Collider col in colliders)
            {
                if (col.name.ToLower().Contains("walls"))
                {
                    isWall = true;
                    break;
                }
            }
            
            if (!isWall)
            {
                Instantiate(batteryPrefab, currentPos, Quaternion.identity, transform);
                spawnedCount++;

                foreach (Vector3 dir in directions)
                {
                    Vector3 neighbor = currentPos + dir * step;
                    
                    neighbor.x = Mathf.Round(neighbor.x * 10f) / 10f;
                    neighbor.z = Mathf.Round(neighbor.z * 10f) / 10f;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}