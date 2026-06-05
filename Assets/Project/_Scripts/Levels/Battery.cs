using UnityEngine;
using Random = UnityEngine.Random;

public class Battery : MonoBehaviour
{
    [Header("Visual Animation")]
    public float rotationSpeed = 80.0f;
    public float floatSpeed = 2.5f;
    public float floatAmplitude = 0.1f;

    private PlayerController player;
    private Vector3 startPos;

    public string Key;
    
    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        startPos = transform.position;
        
        transform.rotation = Quaternion.Euler(15f, Random.Range(0f, 360f), 15f);
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance > player.magnetRadius)
            {
                float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            else
            {
                startPos = transform.position;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.CollectBattery(Key);
            Destroy(gameObject);
        }
    }
}