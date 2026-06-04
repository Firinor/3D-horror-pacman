using UnityEngine;

public class BatteryEditorSpawner : MonoBehaviour
{
    public Transform point1;
    public Transform point2;

    public Transform batteryPool;

    public Battery batteryPrefab;

    public int Xcount;
    public int Ycount;
    public float Ylevel;
    
    [ContextMenu(nameof(GenerateBatteryes))]
    public void GenerateBatteryes()
    {
        batteryPool.ClearAllChildren();

        float Xstep = point2.position.x - point1.position.x;
        Xstep /= Xcount-1;
        float Ystep = point2.position.z - point1.position.z;
        Ystep /= Ycount-1;
        
        float StartX = Mathf.Min(point2.position.x, point1.position.x);
        float StartY = Mathf.Min(point2.position.z, point1.position.z);
        
        for (int i = 0; i < Xcount; i++)
        {
            for (int j = 0; j < Ycount; j++)
            {
                Instantiate(
                    batteryPrefab,
                    new Vector3(
                        StartX + Xstep * i,
                        Ylevel,
                        StartY + Ystep * j),
                    Quaternion.identity,
                    batteryPool);
            }
        }
    }
}
