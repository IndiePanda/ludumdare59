using System;
using UnityEngine;
using Zenject;

public class RecieverZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    [SerializeField] private AudioSource _signalAudioSource;
    private bool _isBroken;
    private EnergySystem _energySystem;
    private SignalSystem _signalSystem;
    private bool _isCharacterInsideZone;

    public event Action ZoneEntered;
    public event Action ZoneExited;
    public event Action<bool> InteractionAvailabilityChanged;

    [Inject]
    private void Construct(EnergySystem energySystem, SignalSystem signalSystem)
    {
        _energySystem = energySystem;
        _signalSystem = signalSystem;
        _energySystem.ChangeEnergy += OnEnergyChanged;
        _signalSystem.SignalAvailabilityChanged += OnSignalAvailabilityChanged;
        UpdateSignalAudio();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = true;
        UpdateSignalAudio();
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
        UpdateSignalAudio();
        ZoneExited?.Invoke();
    }

    private void OnDestroy()
    {
        if (_energySystem != null)
        {
            _energySystem.ChangeEnergy -= OnEnergyChanged;
        }

        if (_signalSystem != null)
        {
            _signalSystem.SignalAvailabilityChanged -= OnSignalAvailabilityChanged;
        }

        if (_signalAudioSource != null && _signalAudioSource.isPlaying)
        {
            _signalAudioSource.Stop();
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

    private void OnSignalAvailabilityChanged()
    {
        UpdateSignalAudio();
    }

    private void UpdateSignalAudio()
    {
        if (_signalAudioSource == null)
        {
            return;
        }

        bool shouldPlay = _signalSystem != null && _signalSystem.HasPendingSignal && !_isCharacterInsideZone;
        if (shouldPlay)
        {
            if (!_signalAudioSource.isPlaying)
            {
                _signalAudioSource.Play();
            }

            return;
        }

        if (_signalAudioSource.isPlaying)
        {
            _signalAudioSource.Stop();
        }
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
