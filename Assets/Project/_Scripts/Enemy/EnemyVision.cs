using UnityEngine;
using System.Collections.Generic;

public class EnemyVision : MonoBehaviour
{
    public float viewAngle = 90f;
    public int viewRays = 20;
    public float viewRadius = 10f;
    public LayerMask obstacleMask;
    
    public bool showGizmos = true;
    public bool createVisibleMesh = true;
    public Material visionMaterial;
    
    public bool canSeeTarget = false;
    public Transform target;
    
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    private Mesh visionMesh;
    
    void Start()
    {
        if (createVisibleMesh)
        {
            meshRenderer.material = visionMaterial;
        }
    }
    
    void Update()
    {
        CheckTargetVisibility();
        
        if (createVisibleMesh)
            UpdateVisionMesh();
    }
    
    void CheckTargetVisibility()
    {
        if (target == null) 
            return;
        
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        directionToTarget.y = 0.1f;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= viewRadius && Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
        {
            if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, viewRadius, obstacleMask))
            {
                canSeeTarget = hit.transform == target;
            }
            else
            {
                canSeeTarget = true;
            }
        }
        else
        {
            canSeeTarget = false;
        }
    }
    
    void UpdateVisionMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        vertices.Add(Vector3.zero);
        
        for (int i = 0; i <= viewRays; i++)
        {
            float angle = -viewAngle / 2 + (viewAngle * i / viewRays);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * transform.forward;
            
            float currentRadius = viewRadius;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask))
                currentRadius = hit.distance;
            
            vertices.Add(rotation * Vector3.forward * currentRadius);
            
            if (i > 0)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }
        
        if (visionMesh == null)
            visionMesh = new Mesh();
        
        visionMesh.Clear();
        visionMesh.vertices = vertices.ToArray();
        visionMesh.triangles = triangles.ToArray();
        visionMesh.RecalculateNormals();
        
        meshFilter.mesh = visionMesh;
    }
    
    public bool IsCanSeeTarget()
    {
        return canSeeTarget;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) 
            return;
        
        Gizmos.color = canSeeTarget ? Color.red : Color.green;
        
        Vector3 forward = transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);
        
        Vector3 previousPoint = transform.position + leftBoundary * viewRadius;
        for (int i = 1; i <= viewRays; i++)
        {
            float angle = -viewAngle / 2 + (viewAngle * i / viewRays);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 currentPoint = transform.position + direction * viewRadius;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        
        if (canSeeTarget && target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i <= viewRays; i++)
        {
            float angle = -viewAngle / 2 + (viewAngle * i / viewRays);
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * transform.forward;
            
            float distance = viewRadius;
            
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, viewRadius, obstacleMask))
            {
                distance = hit.distance;
            }
            
            Gizmos.DrawLine(transform.position, transform.position + direction * distance);
        }
    }
}