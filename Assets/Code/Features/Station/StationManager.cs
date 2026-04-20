using System;
using UnityEngine;
using Zenject;

public class StationManager : MonoBehaviour
{
    [SerializeField] private RecieverZone _recieverZone;
    [SerializeField] private DecoderZone _decoderZone;
    [SerializeField] private EnergyZone _energyZone;
    [SerializeField] private SenderZone _senderZone;

    private TriggerPopupHandler _triggerPopupHandler;
    private SignalSystem _signalSystem;
    private bool _canInteractWithRecieverZone;
    private StationZoneType? _currentZoneType;
    private bool _isCurrentZoneInteractive;
    public event Action ZoneEntered;
    public event Action ZoneExited;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
    }

    private void OnEnable()
    {
        _signalSystem.SignalAvailabilityChanged += OnSignalAvailabilityChanged;
        _recieverZone.InteractionAvailabilityChanged += OnRecieveZoneInteractionAvailabilityChanged;
        _recieverZone.ZoneEntered += OnRecieveZoneZoneEntered;
        _recieverZone.ZoneExited += OnRecieveZoneZoneExited;

        _decoderZone.ZoneEntered += OnDecoderZoneEntered;
        _decoderZone.ZoneExited += OnDecoderZoneExited;

        _senderZone.ZoneEntered += OnSenderZoneEntered;
        _senderZone.ZoneExited += OnSenderZoneExited;

        _energyZone.ZoneEntered += OnEnergyZoneEntered;
        _energyZone.ZoneExited += OnEnergyZoneExited;

    }

    private void OnDisable()
    {
        _signalSystem.SignalAvailabilityChanged -= OnSignalAvailabilityChanged;
        _recieverZone.InteractionAvailabilityChanged -= OnRecieveZoneInteractionAvailabilityChanged;
        _recieverZone.ZoneEntered -= OnRecieveZoneZoneEntered;
        _recieverZone.ZoneExited -= OnRecieveZoneZoneExited;

        _decoderZone.ZoneEntered -= OnDecoderZoneEntered;
        _decoderZone.ZoneExited -= OnDecoderZoneExited;

        _senderZone.ZoneEntered -= OnSenderZoneEntered;
        _senderZone.ZoneExited -= OnSenderZoneExited;

        _energyZone.ZoneEntered -= OnEnergyZoneEntered;
        _energyZone.ZoneExited -= OnEnergyZoneExited;

        ClearCurrentZone();
    }

    private void Update()
    {
        if (_currentZoneType == null || _triggerPopupHandler.IsPopupOpen || !_isCurrentZoneInteractive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            _triggerPopupHandler.Open(_currentZoneType.Value);
        }
    }

    private void OnSignalAvailabilityChanged()
    {
        RefreshCurrentZoneState();
        Debug.Log("RefreshCurrentZoneState");
    }


    private void OnRecieveZoneZoneEntered()
    {
        SetCurrentZone(StationZoneType.Reciever);
    }

    private void OnRecieveZoneInteractionAvailabilityChanged(bool canInteract)
    {
        _canInteractWithRecieverZone = canInteract;
        RefreshCurrentZoneState();
    }

    private void OnRecieveZoneZoneExited()
    {
        ClearCurrentZone(StationZoneType.Reciever);
    }

    private void OnEnergyZoneEntered()
    {
        SetCurrentZone(StationZoneType.Energy);
    }

    private void OnEnergyZoneExited()
    {
        ClearCurrentZone(StationZoneType.Energy);
    }

    private void OnDecoderZoneEntered()
    {
        SetCurrentZone(StationZoneType.Decoder);
    }

    private void OnDecoderZoneExited()
    {
        ClearCurrentZone(StationZoneType.Decoder);
    }

    private void OnSenderZoneEntered()
    {
        SetCurrentZone(StationZoneType.Sender);
    }

    private void OnSenderZoneExited()
    {
        ClearCurrentZone(StationZoneType.Sender);
    }

    private void SetCurrentZone(StationZoneType zoneType)
    {
        _currentZoneType = zoneType;
        RefreshCurrentZoneState();
    }

    private void ClearCurrentZone(StationZoneType zoneType)
    {
        if (_currentZoneType != zoneType)
        {
            return;
        }

        ClearCurrentZone();
    }

    private void ClearCurrentZone()
    {
        if (_isCurrentZoneInteractive)
        {
            _isCurrentZoneInteractive = false;
            ZoneExited?.Invoke();
        }

        _currentZoneType = null;
    }

    private void RefreshCurrentZoneState()
    {
        bool isCurrentZoneInteractive = CanInteractWithCurrentZone();

        if (_isCurrentZoneInteractive == isCurrentZoneInteractive)
        {
            return;
        }

        _isCurrentZoneInteractive = isCurrentZoneInteractive;

        if (_isCurrentZoneInteractive)
        {
            ZoneEntered?.Invoke();
            return;
        }

        ZoneExited?.Invoke();
    }

    private bool CanInteractWithCurrentZone()
    {
        if (_currentZoneType == null)
        {
            return false;
        }

        if (_currentZoneType != StationZoneType.Reciever)
        {
            if (_currentZoneType == StationZoneType.Decoder)
            {
                return _signalSystem.HasPendingDecoding;
            }

            if (_currentZoneType == StationZoneType.Sender)
            {
                return _signalSystem.HasPendingSending;
            }

            return true;
        }

        return _signalSystem.HasPendingSignal && _canInteractWithRecieverZone;
    }

}
