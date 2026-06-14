using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSteps : MonoBehaviour
{
    [SerializeField] 
    private AudioConfig sounds;
    [SerializeField] 
    private AudioSource _audioSource;

    public float StepDistance;
    
    private float stepDistanceLeft;
    private Vector3 lastPosition;
    
    private void Start()
    {
        lastPosition = transform.position;
        stepDistanceLeft = StepDistance;
        
        _audioSource = GetComponentInParent<AudioSource>();
    }

    public void Update()
    {
        float distanceDelta = Vector3.Distance(lastPosition, transform.position);
        lastPosition = transform.position;
        stepDistanceLeft -= distanceDelta;

        if (stepDistanceLeft <= 0)
        {
            stepDistanceLeft = StepDistance;
            sounds.Steps.Play(_audioSource);
        }
    }
}