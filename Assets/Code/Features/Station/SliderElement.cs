using UnityEngine;
using Zenject;

public class SliderElement : Element
{
    private const float SliderRange = 710f;

    [SerializeField] private int _positionCount;
    [SerializeField] private Transform _sliderHandle;

    private RectTransform _sliderHandleRectTransform;
    private Vector3 _defaultLocalPosition;
    private Vector2 _defaultAnchoredPosition;
    private int _currentPosition;
    private SFXAudio _sfxAudio;

    public int CurrentPosition => _currentPosition;

    protected override void CacheReferences()
    {
        if (_sliderHandle == null)
        {
            _sliderHandle = transform.Find("SliderHandle");
        }

        if (_sliderHandle == null)
        {
            return;
        }

        _defaultLocalPosition = _sliderHandle.localPosition;
        _sliderHandleRectTransform = _sliderHandle as RectTransform;
        if (_sliderHandleRectTransform != null)
        {
            _defaultAnchoredPosition = _sliderHandleRectTransform.anchoredPosition;
        }
    }

    protected override void SetDefaultValue()
    {
        _currentPosition = GetFirstPosition();
        ApplyVisualState();
    }

    protected override void ChangeValueInternal(Vector2Int direction)
    {
        if (!TryChangePosition(ref _currentPosition, _positionCount, direction.x))
        {
            return;
        }

        ApplyVisualState();
        _sfxAudio?.PlaySwitch();
    }

    [Inject]
    private void Construct(SFXAudio sfxAudio)
    {
        _sfxAudio = sfxAudio;
    }

    private void ApplyVisualState()
    {
        if (_sliderHandle == null)
        {
            return;
        }

        float normalizedValue = GetNormalizedValue(_currentPosition, _positionCount);
        float offset = normalizedValue * SliderRange;

        if (_sliderHandleRectTransform != null)
        {
            _sliderHandleRectTransform.anchoredPosition = _defaultAnchoredPosition + new Vector2(offset, 0f);
            return;
        }

        _sliderHandle.localPosition = _defaultLocalPosition + new Vector3(offset, 0f, 0f);
    }
}
