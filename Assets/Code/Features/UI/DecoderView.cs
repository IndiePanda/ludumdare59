using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DecoderView : MonoBehaviour
{
    private const int Switch01Index = 0;
    private const int Switch02Index = 1;
    private const int Slider01Index = 2;
    private const int Slider02Index = 3;

    [SerializeField] private DecoderPresenter _presenter;
    [SerializeField] private Element[] _elementList;
    [SerializeField] private TextMeshProUGUI _value;
    [SerializeField] private Image _signalType;
    [SerializeField] private RawImage _signalImage;
    [SerializeField] private float _signalScrollSpeed = 0.1f;
    [SerializeField] private Image[] _signalDetectorList;
    [SerializeField] private Sprite _activeSignalDetectorSprite;
    [SerializeField] private Sprite _inactiveSignalDetectorSprite;
    [SerializeField] private Sprite[] _signalTypeList;

    private TriggerPopupHandler _triggerPopupHandler;
    private SignalSystem _signalSystem;
    private CompletePanelView _completePanel;

    private int _activeElementIndex = -1;
    private Rect _uvRect;
    private bool _isCompleting;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem, CompletePanelView completePanel)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
        _completePanel = completePanel;
    }

    void Start()
    {
        SetActiveElement(GetFirstAvailableElementIndex());
        _uvRect = _signalImage.uvRect;
        UpdateDecoderState();

        if (_completePanel != null)
        {
            _completePanel.gameObject.SetActive(false);
        }
    }

    private void ClosePopup()
    {
        _triggerPopupHandler.CloseCurrent();
        Destroy(gameObject);
    }

    void Update()
    {
        if (_isCompleting)
        {
            return;
        }

        _uvRect.x += _signalScrollSpeed * Time.deltaTime;
        _signalImage.uvRect = _uvRect;

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
                UpdateDecoderState();
                return;
            }

            activeElement.ApplyInput(elementInputDirection);
            UpdateDecoderState();
            return;
        }

        if (elementInputDirection != Vector2Int.zero)
        {
            activeElement.ApplyInput(elementInputDirection);
            UpdateDecoderState();
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

    private void UpdateDecoderState()
    {
        if (_signalSystem == null || !_signalSystem.TryGetCurrentDecoderCombination(out SignalSystem.DecoderCombination decoderCombination))
        {
            SetAllDetectorsInactive();
            return;
        }

        int signalTypeValue = Mathf.Max(0, GetElementPosition(Switch01Index) - 1);
        int signalValue = GetElementPosition(Switch02Index) * 16;
        int wavelengthValue = Mathf.Max(0, GetElementPosition(Slider01Index) - 1);
        int waveHeightValue = Mathf.Max(0, GetElementPosition(Slider02Index) - 1);

        UpdateSignalType(signalTypeValue);
        UpdateSignalValue(signalValue);

        SetDetectorState(0, signalTypeValue == decoderCombination.SignalType);
        SetDetectorState(1, signalValue == decoderCombination.SignalValue);
        SetDetectorState(2, wavelengthValue == decoderCombination.Wavelength);
        SetDetectorState(3, waveHeightValue == decoderCombination.WaveHeight);

        if (signalTypeValue == decoderCombination.SignalType
            && signalValue == decoderCombination.SignalValue
            && wavelengthValue == decoderCombination.Wavelength
            && waveHeightValue == decoderCombination.WaveHeight
            && _signalSystem.TryCompleteDecoding())
        {
            CompleteAndClose();
        }
    }

    private void CompleteAndClose()
    {
        if (_isCompleting)
        {
            return;
        }

        _isCompleting = true;

        if (_completePanel != null)
        {
            _completePanel.gameObject.SetActive(true);
        }
        ClosePopup();
    }

    private void UpdateSignalType(int signalTypeValue)
    {
        if (_signalTypeList == null || signalTypeValue < 0 || signalTypeValue >= _signalTypeList.Length)
        {
            return;
        }

        Sprite signalTypeSprite = _signalTypeList[signalTypeValue];
        if (_signalType != null)
        {
            _signalType.sprite = signalTypeSprite;
        }

        if (_signalImage != null)
        {
            _signalImage.texture = signalTypeSprite != null ? signalTypeSprite.texture : null;
        }
    }

    private void UpdateSignalValue(int signalValue)
    {
        if (_value != null)
        {
            _value.text = signalValue.ToString();
        }
    }

    private void SetDetectorState(int detectorIndex, bool isActive)
    {
        if (_signalDetectorList == null || detectorIndex < 0 || detectorIndex >= _signalDetectorList.Length)
        {
            return;
        }

        Image detector = _signalDetectorList[detectorIndex];
        if (detector == null)
        {
            return;
        }

        detector.sprite = isActive ? _activeSignalDetectorSprite : _inactiveSignalDetectorSprite;
    }

    private void SetAllDetectorsInactive()
    {
        if (_signalDetectorList == null)
        {
            return;
        }

        for (int i = 0; i < _signalDetectorList.Length; i++)
        {
            SetDetectorState(i, false);
        }
    }

    private int GetElementPosition(int elementIndex)
    {
        if (_elementList == null || elementIndex < 0 || elementIndex >= _elementList.Length)
        {
            return 0;
        }

        Element element = _elementList[elementIndex];
        return element switch
        {
            RoundSwitchElement roundSwitchElement => roundSwitchElement.CurrentPosition,
            SwitchElement switchElement => switchElement.CurrentPosition,
            SliderElement sliderElement => sliderElement.CurrentPosition,
            _ => 0,
        };
    }
}
