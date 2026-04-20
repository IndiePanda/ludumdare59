using System;
using UnityEngine;

public class MusicBoxZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    [SerializeField] private float _restoreCooldown;
    private bool _isBroken;
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

        if (_isBroken || Time.time < _nextRestoreTime)
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

    private void Broke()
    {
        _isBroken = true;
    }

    private void Repair()
    {
        _isBroken = false;
    }
}
