using System;
using UnityEngine;

public class EnergyZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    [SerializeField] private float _restoreCooldown;
    private float _nextRestoreTime;
    public event Action ZoneEntered;
    public event Action ZoneExited;
    public event Action EnergyRestoreRequested;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        if (Time.time < _nextRestoreTime)
        {
            return;
        }

        _nextRestoreTime = Time.time + _restoreCooldown;
        EnergyRestoreRequested?.Invoke();

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
}
