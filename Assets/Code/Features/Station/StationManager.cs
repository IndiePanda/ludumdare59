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
    private EnergySystem _energySystem;
    private StationZoneType? _currentZoneType;
    private bool _isCurrentZoneInteractive;
    public event Action ZoneEntered;
    public event Action ZoneExited;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem, EnergySystem energySystem)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
        _energySystem = energySystem;
    }

    private void OnEnable()
    {
        _signalSystem.SignalAvailabilityChanged += OnSignalAvailabilityChanged;
        _energySystem.ChangeEnergy += OnEnergyChanged;
        _recieverZone.ZoneEntered += OnRecieveZoneZoneEntered;
        _recieverZone.ZoneExited += OnRecieveZoneZoneExited;

        _decoderZone.ZoneEntered += OnDecoderZoneEntered;
        _decoderZone.ZoneExited += OnDecoderZoneExited;

        _senderZone.ZoneEntered += OnSenderZoneEntered;
        _senderZone.ZoneExited += OnSenderZoneExited;

        _energyZone.AvailabilityChanged += OnEnergyZoneAvailabilityChanged;
        _energyZone.ZoneEntered += OnEnergyZoneEntered;
        _energyZone.ZoneExited += OnEnergyZoneExited;

        UpdateZoneFrames();
    }

    private void OnDisable()
    {
        _signalSystem.SignalAvailabilityChanged -= OnSignalAvailabilityChanged;
        _energySystem.ChangeEnergy -= OnEnergyChanged;
        _recieverZone.ZoneEntered -= OnRecieveZoneZoneEntered;
        _recieverZone.ZoneExited -= OnRecieveZoneZoneExited;

        _decoderZone.ZoneEntered -= OnDecoderZoneEntered;
        _decoderZone.ZoneExited -= OnDecoderZoneExited;

        _senderZone.ZoneEntered -= OnSenderZoneEntered;
        _senderZone.ZoneExited -= OnSenderZoneExited;

        _energyZone.AvailabilityChanged -= OnEnergyZoneAvailabilityChanged;
        _energyZone.ZoneEntered -= OnEnergyZoneEntered;
        _energyZone.ZoneExited -= OnEnergyZoneExited;

        ClearCurrentZone();
        SetZoneFrameActive(_recieverZone, false);
        SetZoneFrameActive(_decoderZone, false);
        SetZoneFrameActive(_energyZone, false);
        SetZoneFrameActive(_senderZone, false);
    }

    private void Update()
    {
        if (_currentZoneType == null || _triggerPopupHandler.IsPopupOpen || !_isCurrentZoneInteractive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_currentZoneType.Value == StationZoneType.Energy)
            {
                _energyZone.TryCollectZone();
            }
            else
            {
                _triggerPopupHandler.Open(_currentZoneType.Value);
            }
            
        }
    }

    private void OnSignalAvailabilityChanged()
    {
        RefreshCurrentZoneState();
        UpdateZoneFrames();
    }

    private void OnEnergyChanged(int currentEnergy)
    {
        RefreshCurrentZoneState();
        UpdateZoneFrames();
    }

    private void OnEnergyZoneAvailabilityChanged(bool isAvailable)
    {
        RefreshCurrentZoneState();
        UpdateZoneFrames();
    }


    private void OnRecieveZoneZoneEntered()
    {
        SetCurrentZone(StationZoneType.Reciever);
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

        return CanInteractWithZone(_currentZoneType.Value);
    }

    private bool CanInteractWithZone(StationZoneType zoneType)
    {
        if (zoneType == StationZoneType.Reciever)
        {
            return _signalSystem.HasPendingSignal && _energySystem.CurrentEnergy > 0;
        }

        if (zoneType == StationZoneType.Decoder)
        {
            return _signalSystem.HasPendingDecoding;
        }

        if (zoneType == StationZoneType.Sender)
        {
            return _signalSystem.HasPendingSending;
        }

        return _energyZone != null && _energyZone.IsAvailable;
    }

    private void UpdateZoneFrames()
    {
        SetZoneFrameActive(_recieverZone, CanInteractWithZone(StationZoneType.Reciever));
        SetZoneFrameActive(_decoderZone, CanInteractWithZone(StationZoneType.Decoder));
        SetZoneFrameActive(_energyZone, CanInteractWithZone(StationZoneType.Energy));
        SetZoneFrameActive(_senderZone, CanInteractWithZone(StationZoneType.Sender));
    }

    private static void SetZoneFrameActive(MonoBehaviour zone, bool isActive)
    {
        if (zone == null || zone.transform.childCount == 0)
        {
            return;
        }

        zone.transform.GetChild(0).gameObject.SetActive(isActive);
    }

}
