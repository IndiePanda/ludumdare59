using UnityEngine;
using Zenject;

public class TriggerPopupHandler
{
    private readonly UIConfig _UIConfig;
    private readonly Transform _canvas;
    private readonly DiContainer _container;

    private GameObject _currentPopup;
    private StationZoneType? _currentPopupType;
    public bool IsPopupOpen => _currentPopup != null;

    public TriggerPopupHandler(UIConfig UIConfig, [Inject(Id = "CANVAS")] Transform canvas, DiContainer container)
    {
        _UIConfig = UIConfig;
        _canvas = canvas;
        _container = container;
    }

    public void Open(StationZoneType zoneType)
    {
        if (_currentPopup != null && _currentPopupType == zoneType)
        {
            return;
        }

        CloseCurrent();

        GameObject popupPrefab = GetPopupPrefab(zoneType);
        if (popupPrefab == null)
        {
            return;
        }

        _currentPopup = _container.InstantiatePrefab(popupPrefab, _canvas);
        _currentPopupType = zoneType;
    }

    private GameObject GetPopupPrefab(StationZoneType zoneType)
    {
        if (_UIConfig.Popups == null)
        {
            return null;
        }

        return zoneType switch
        {
            StationZoneType.Reciever => GetPopupByIndex(0),
            StationZoneType.Decoder => GetPopupByIndex(1),
            StationZoneType.Sender => GetPopupByIndex(2),
            _ => null,
        };
    }

    private GameObject GetPopupByIndex(int index)
    {
        if (index < 0 || index >= _UIConfig.Popups.Length)
        {
            return null;
        }

        return _UIConfig.Popups[index];
    }

    public void CloseCurrent()
    {
        if (_currentPopup != null)
        {
            Object.Destroy(_currentPopup);
            _currentPopup = null;
        }

        _currentPopupType = null;
    }
}
