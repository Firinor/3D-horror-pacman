using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    [SerializeField] 
    private AudioConfig sounds;
    [SerializeField] 
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponentInParent<AudioSource>();
    }

    public void OnStep()
    {
        if (_audioSource == null) return;

        sounds.Steps.Play(_audioSource);
    }
}