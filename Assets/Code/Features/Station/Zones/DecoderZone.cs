using System;
using UnityEngine;

public class DecoderZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    private bool _isBroken;
    public event Action ZoneEntered;
    public event Action ZoneExited;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        ZoneEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        ZoneExited?.Invoke();
    }

    private void Broke()
    {
        _isBroken = true;
    }

    private void Repair()
    {
        _isBroken = false;
    }
}
