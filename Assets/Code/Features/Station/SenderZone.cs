using System;
using UnityEngine;
using Zenject;

public class SenderZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    private bool _isBroken;
    private EnergySystem _energySystem;
    private bool _isCharacterInsideZone;

    public event Action ZoneEntered;
    public event Action ZoneExited;

    [Inject]
    private void Construct(EnergySystem energySystem)
    {
        _energySystem = energySystem;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = true;
        ZoneEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = false;
        ZoneExited?.Invoke();
    }
}
