using UnityEngine;

public class Biha_Enemy : MonoBehaviour
{
    [SerializeField] 
    private Transform player;
    
    void Update()
    {
        transform.LookAt(player);
    }
}
