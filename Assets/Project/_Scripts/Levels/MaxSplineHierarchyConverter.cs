using UnityEngine;

public class MaxSplineGlobalSpawner : MonoBehaviour
{
    public GameObject batteryPrefab;
    public float stepBetweenItems = 2.0f;
    
    public bool preventOverlap = true;
    public float overlapRadius = 1.0f;
    public LayerMask batteryLayer;
    
    public void SpawnEverything()
    {
        if (batteryPrefab == null)
        { ;
            return;
        }

        int totalSpawned = 0;
        
        foreach (Transform lineTransform in transform)
        {
            Transform[] points = lineTransform.GetComponentsInChildren<Transform>();
            
            if (points.Length < 2) continue;
            
            for (int i = 0; i < points.Length - 1; i++)
            {
                if (points[i] == lineTransform || points[i + 1] == lineTransform) continue;

                totalSpawned += BuildRowBetweenTwoPoints(points[i].position, points[i + 1].position);
            }
        }
    }

    int BuildRowBetweenTwoPoints(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        if (distance < stepBetweenItems) return 0;

        int count = Mathf.FloorToInt(distance / stepBetweenItems);
        Vector3 direction = (end - start).normalized;
        int spawnedCount = 0;

        for (int i = 0; i <= count; i++)
        {
            Vector3 spawnPos = start + (direction * (i * stepBetweenItems));

            if (preventOverlap)
            {
                if (Physics.CheckSphere(spawnPos, overlapRadius, batteryLayer))
                {
                    continue;
                }
            }

            GameObject item = Instantiate(batteryPrefab, spawnPos, Quaternion.identity);
            
            item.transform.parent = this.transform;
            spawnedCount++;
        }

        return spawnedCount;
    }
}