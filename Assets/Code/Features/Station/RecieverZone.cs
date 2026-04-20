using System;
using UnityEngine;
using Zenject;

public class RecieverZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    private bool _isBroken;
    private EnergySystem _energySystem;
    private bool _isCharacterInsideZone;

    public event Action ZoneEntered;
    public event Action ZoneExited;
    public event Action<bool> InteractionAvailabilityChanged;

    [Inject]
    private void Construct(EnergySystem energySystem)
    {
        _energySystem = energySystem;
        _energySystem.ChangeEnergy += OnEnergyChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = true;
        NotifyInteractionAvailabilityChanged();
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

    private void OnDestroy()
    {
        if (_energySystem != null)
        {
            _energySystem.ChangeEnergy -= OnEnergyChanged;
        }
    }

    private void OnEnergyChanged(int value)
    {
        if (!_isCharacterInsideZone)
        {
            return;
        }

        NotifyInteractionAvailabilityChanged(value);
    }

    private void NotifyInteractionAvailabilityChanged()
    {
        NotifyInteractionAvailabilityChanged(_energySystem != null && _energySystem.CurrentEnergy > 0);
    }

    private void NotifyInteractionAvailabilityChanged(int currentEnergy)
    {
        NotifyInteractionAvailabilityChanged(currentEnergy > 0);
    }

    private void NotifyInteractionAvailabilityChanged(bool canInteract)
    {
        InteractionAvailabilityChanged?.Invoke(canInteract);
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
