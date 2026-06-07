using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform target;

    public void Initialize(Transform target)
    {
        this.target = target;
    }
    
    void Update()
    {
        Vector3 point = target.position;
        point.y = transform.position.y;
        transform.LookAt(worldPosition: point);
    }
}
