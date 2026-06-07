using System;
using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public event Action OnPlayerEnter;
    
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
            return;

        OnPlayerEnter.Invoke();
    }
}
