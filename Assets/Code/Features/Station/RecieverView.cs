using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class RecieverView : MonoBehaviour
{
    private const float RadarUvSensitivity = 20f;
    private const int DetectorStep = 20;

    [SerializeField] private Element[] _elementList;
    [SerializeField] private TextMeshProUGUI _coordinatesValue;
    [SerializeField] private TextMeshProUGUI _distValue;
    [SerializeField] private RawImage _radarImage;
    [SerializeField] private Image[] _signalDetectorList;
    [SerializeField] private Sprite _activeSignalDetectorSprite;
    [SerializeField] private Sprite _inactiveSignalDetectorSprite;

    private TriggerPopupHandler _triggerPopupHandler;
    private SignalSystem _signalSystem;

    private int _activeElementIndex = -1;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
    }

    void Start()
    {
        SetActiveElement(GetFirstAvailableElementIndex());
        UpdateCoordinatesValue();
    }

    private void ClosePopup()
    {
        _triggerPopupHandler.CloseCurrent();
        Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClosePopup();
            return;
        }

        Vector2 navigationDirection = GetNavigationDirection();
        if (navigationDirection != Vector2.zero)
        {
            MoveSelection(navigationDirection);
            return;
        }

        if (_activeElementIndex < 0 || _activeElementIndex >= _elementList.Length)
        {
            return;
        }

        Element activeElement = _elementList[_activeElementIndex];
        if (activeElement == null)
        {
            return;
        }

        Vector2Int elementInputDirection = GetElementInputDirection(activeElement.UsesHoldInput);
        if (activeElement.UsesHoldInput)
        {
            if (elementInputDirection == Vector2Int.zero)
            {
                activeElement.ReleaseInput();
                UpdateCoordinatesValue();
                return;
            }

            activeElement.ApplyInput(elementInputDirection);
            UpdateCoordinatesValue();
            return;
        }

        if (elementInputDirection != Vector2Int.zero)
        {
            activeElement.ApplyInput(elementInputDirection);
            UpdateCoordinatesValue();
        }
    }

    private Vector2 GetNavigationDirection()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            return Vector2.left;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            return Vector2.right;
        }

        return Vector2.zero;
    }

    private Vector2Int GetElementInputDirection(bool useHoldInput)
    {
        int horizontal = 0;
        int vertical = 0;

        bool leftPressed = useHoldInput ? Input.GetKey(KeyCode.A) : Input.GetKeyDown(KeyCode.A);
        bool rightPressed = useHoldInput ? Input.GetKey(KeyCode.D) : Input.GetKeyDown(KeyCode.D);
        bool downPressed = useHoldInput ? Input.GetKey(KeyCode.S) : Input.GetKeyDown(KeyCode.S);
        bool upPressed = useHoldInput ? Input.GetKey(KeyCode.W) : Input.GetKeyDown(KeyCode.W);

        if (leftPressed)
        {
            horizontal = -1;
        }
        else if (rightPressed)
        {
            horizontal = 1;
        }

        if (downPressed)
        {
            vertical = -1;
        }
        else if (upPressed)
        {
            vertical = 1;
        }

        return new Vector2Int(horizontal, vertical);
    }

    private void MoveSelection(Vector2 direction)
    {
        int currentIndex = _activeElementIndex >= 0 ? _activeElementIndex : GetFirstAvailableElementIndex();
        int step = direction.x > 0f ? 1 : -1;
        int nextIndex = currentIndex;
        int checkedCount = 0;

        while (checkedCount < _elementList.Length)
        {
            nextIndex = (nextIndex + step + _elementList.Length) % _elementList.Length;

            if (_elementList[nextIndex] != null)
            {
                SetActiveElement(nextIndex);
                return;
            }

            checkedCount++;
        }
    } 
    
    private void SetActiveElement(int elementIndex)
    {
        if (_activeElementIndex >= 0 && _activeElementIndex < _elementList.Length && _elementList[_activeElementIndex] != null)
        {
            _elementList[_activeElementIndex].SetActive(false);
        }

        _activeElementIndex = elementIndex;
        _elementList[_activeElementIndex].SetActive(true);
        UpdateCoordinatesValue();
    }

    private int GetFirstAvailableElementIndex()
    {
        for (int i = 0; i < _elementList.Length; i++)
        {
            if (_elementList[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private void UpdateCoordinatesValue()
    {
        JoystickElement joystickElement = GetJoystickElement();
        if (_coordinatesValue != null)
        {
            _coordinatesValue.text = joystickElement != null ? joystickElement.GetCoordinatesText() : string.Empty;
        }

        UpdateRadarImage(joystickElement);
        UpdateSignalDetectors(joystickElement);
    }

    private JoystickElement GetJoystickElement()
    {
        for (int i = 0; i < _elementList.Length; i++)
        {
            if (_elementList[i] is JoystickElement joystickElement)
            {
                return joystickElement;
            }
        }

        return null;
    }

    private void UpdateRadarImage(JoystickElement joystickElement)
    {
        if (_radarImage == null || joystickElement == null)
        {
            return;
        }

        Rect uvRect = _radarImage.uvRect;
        float maxX = Mathf.Max(0f, 1f - uvRect.width);
        float maxY = Mathf.Max(0f, 1f - uvRect.height);
        float centeredLongitude = joystickElement.CurrentLongitudeNormalized - 0.5f;
        float centeredLatitude = joystickElement.CurrentLatitudeNormalized - 0.5f;

        uvRect.x = maxX * 0.5f + centeredLongitude * maxX * RadarUvSensitivity;
        uvRect.y = maxY * 0.5f + centeredLatitude * maxY * RadarUvSensitivity;

        _radarImage.uvRect = uvRect;
    }

    private void UpdateSignalDetectors(JoystickElement joystickElement)
    {
        if (_signalDetectorList == null || _signalDetectorList.Length == 0)
        {
            return;
        }

        int activeDetectorCount = 0;
        if (joystickElement != null && _signalSystem != null && _signalSystem.TryGetCurrentSignalTarget(out SignalSystem.SignalTarget signalTarget))
        {
            Vector2Int currentCoordinates = new(Mathf.RoundToInt(joystickElement.CurrentLongitude), Mathf.RoundToInt(joystickElement.CurrentLatitude));
            Vector2Int targetCoordinates = new(Mathf.RoundToInt(signalTarget.Longitude), Mathf.RoundToInt(signalTarget.Latitude));
            float distance = Vector2Int.Distance(currentCoordinates, targetCoordinates);

            _distValue.text = $"{distance:F4}";

            if (distance <= 1.5f)
            {
                activeDetectorCount = 4;
            }
            else if (distance <= DetectorStep)
            {
                activeDetectorCount = 3;
            }
            else if (distance <= DetectorStep * 2)
            {
                activeDetectorCount = 2;
            }
            else if (distance <= DetectorStep * 3)
            {
                activeDetectorCount = 1;
            }
        }

        for (int i = 0; i < _signalDetectorList.Length; i++)
        {
            Image detector = _signalDetectorList[i];
            if (detector == null)
            {
                continue;
            }

            detector.sprite = i < activeDetectorCount && _activeSignalDetectorSprite != null
                ? _activeSignalDetectorSprite
                : _inactiveSignalDetectorSprite;
        }

        if (activeDetectorCount == 4 && _signalSystem != null && _signalSystem.TryReceiveSignal())
        {
            ClosePopup();
        }
    }
}
