using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NewBehaviourScript : MonoBehaviour
{
    public float lineWidth = 0.3f;
    public GameObject batteryPrefab;
    public float distanceBetweenBatteries = 1.0f;
    public float batteryHeightOffset = 0.2f;
    
    public float minOverlapRadius = 0.6f;

    private void Start()
    {
        HideAllLines();
    }

    [ContextMenu("1 3DS MAX NODES")]
    public void ConvertMaxNodesToSplines()
    {
        Transform batteryHolder = transform.Find("Generated_Batteries");
        if (batteryHolder != null)
        {
            DestroyImmediate(batteryHolder.gameObject);
        }
        GameObject holderObj = new GameObject("Generated_Batteries");
        holderObj.transform.SetParent(transform);
        batteryHolder = holderObj.transform;

        List<Vector3> spawnedPositions = new List<Vector3>();
        int totalBatteriesSpawned = 0;

        foreach (Transform lineContainer in transform)
        {
            if (lineContainer == this.transform || lineContainer.name == "Generated_Batteries") continue;

            List<Transform> waypoints = new List<Transform>();

            for (int i = 0; i < lineContainer.childCount; i++)
            {
                Transform child = lineContainer.GetChild(i);
                if (child.name.ToLower().Contains("node"))
                {
                    waypoints.Add(child);
                }
            }
            
            waypoints = waypoints.OrderBy(w => w.name).ToList();

            if (waypoints.Count < 2) continue;

            //LineRenderer
            LineRenderer lr = lineContainer.GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = lineContainer.gameObject.AddComponent<LineRenderer>();
            }
            
            lr.positionCount = waypoints.Count;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            Vector3[] positions = waypoints.Select(w => w.position).ToArray();
            lr.SetPositions(positions);
            
            float currentDistanceAccumulator = 0f;

            for (int i = 0; i < positions.Length - 1; i++)
            {
                Vector3 startPoint = positions[i];
                Vector3 endPoint = positions[i + 1];
                float segmentLength = Vector3.Distance(startPoint, endPoint);
                Vector3 direction = (endPoint - startPoint).normalized;

                while (currentDistanceAccumulator <= segmentLength)
                {
                    Vector3 spawnPos = startPoint + direction * currentDistanceAccumulator;
                    Vector3 checkPos = spawnPos;

                    spawnPos.y += batteryHeightOffset;
                    
                    bool isTooCloseToAnother = false;
                    foreach (Vector3 existingPos in spawnedPositions)
                    {
                        if (Vector3.Distance(checkPos, existingPos) < minOverlapRadius)
                        {
                            isTooCloseToAnother = true;
                            break;
                        }
                    }
                    
                    if (!isTooCloseToAnother)
                    {
                        GameObject battery = Instantiate(batteryPrefab, spawnPos, Quaternion.identity, batteryHolder);
                        battery.name = $"Battery_{lr.gameObject.name}_{totalBatteriesSpawned}";
                        
                        spawnedPositions.Add(checkPos);
                        totalBatteriesSpawned++;
                    }

                    currentDistanceAccumulator += distanceBetweenBatteries;
                }

                currentDistanceAccumulator -= segmentLength;
            }
        }

        //Debug.Log($"totalBatteriesSpawned {totalBatteriesSpawned}");
    }

    [ContextMenu("2 HideAllLines")]
    public void HideAllLines()
    {
        //LineRenderer
        LineRenderer[] allLineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.enabled = false;
        }
        //Debug.Log($"({allLineRenderers.Length})");
    }

    [ContextMenu("3 ShowAllLines")]
    public void ShowAllLines()
    {
        LineRenderer[] allLineRenderers = GetComponentsInChildren<LineRenderer>(true);
        foreach (LineRenderer lr in allLineRenderers)
        {
            lr.enabled = true;
        }
        //Debug.Log($"({allLineRenderers.Length}) Scene.");
    }
}