using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform target;
    
    void Update()
    {
        Vector3 point = target.position;
        point.y = transform.position.y;
        transform.LookAt(worldPosition: point);
    }
}
