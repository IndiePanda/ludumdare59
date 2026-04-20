using UnityEngine;

public class JoystickElement : Element
{
    private const float SliderRange = 230f;
    private const float MaxLatitude = 90f;
    private const float MaxLongitude = 180f;
    private const float LongitudeChangeSpeed = 2f;
    private const float LatitudeChangeSpeed = LongitudeChangeSpeed * MaxLatitude / MaxLongitude;

    [SerializeField] private int _positionCount;
    [SerializeField] private int _verticalPositionCount = 3;
    [SerializeField] private Transform _sliderHandle;

    private RectTransform _sliderHandleRectTransform;
    private Vector3 _defaultLocalPosition;
    private Vector2 _defaultAnchoredPosition;
    private int _currentHorizontalPosition;
    private int _currentVerticalPosition;
    private float _currentLatitude;
    private float _currentLongitude;

    public override bool UsesHoldInput => true;
    public float CurrentLatitude => _currentLatitude;
    public float CurrentLongitude => _currentLongitude;
    public float CurrentLatitudeNormalized => CurrentLatitude / MaxLatitude;
    public float CurrentLongitudeNormalized => CurrentLongitude / MaxLongitude;

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
        _currentHorizontalPosition = GetCenteredPosition(_positionCount);
        _currentVerticalPosition = GetCenteredPosition(GetVerticalPositionCount());
        _currentLatitude = MaxLatitude * 0.5f;
        _currentLongitude = MaxLongitude * 0.5f;
        ApplyVisualState();
    }

    public override void ApplyInput(Vector2Int direction)
    {
        Initialize();

        int nextHorizontalPosition = GetPressedPosition(direction.x, _positionCount);
        int nextVerticalPosition = GetPressedPosition(direction.y, GetVerticalPositionCount());
        _currentLatitude = Mathf.Clamp(_currentLatitude + direction.y * LatitudeChangeSpeed * Time.deltaTime, 0f, MaxLatitude);
        _currentLongitude = Mathf.Clamp(_currentLongitude + direction.x * LongitudeChangeSpeed * Time.deltaTime, 0f, MaxLongitude);

        _currentHorizontalPosition = nextHorizontalPosition;
        _currentVerticalPosition = nextVerticalPosition;
        ApplyVisualState();
    }

    public override void ReleaseInput()
    {
        Initialize();

        int centeredHorizontalPosition = GetCenteredPosition(_positionCount);
        int centeredVerticalPosition = GetCenteredPosition(GetVerticalPositionCount());
        if (_currentHorizontalPosition == centeredHorizontalPosition && _currentVerticalPosition == centeredVerticalPosition)
        {
            return;
        }

        _currentHorizontalPosition = centeredHorizontalPosition;
        _currentVerticalPosition = centeredVerticalPosition;
        ApplyVisualState();
    }

    protected override void ChangeValueInternal(Vector2Int direction)
    {
        ApplyInput(direction);
    }

    private void ApplyVisualState()
    {
        if (_sliderHandle == null)
        {
            return;
        }

        float normalizedHorizontalValue = GetNormalizedValue(_currentHorizontalPosition, _positionCount);
        float normalizedVerticalValue = GetNormalizedValue(_currentVerticalPosition, GetVerticalPositionCount());
        float horizontalOffset = Mathf.Lerp(-SliderRange * 0.5f, SliderRange * 0.5f, normalizedHorizontalValue);
        float verticalOffset = Mathf.Lerp(-SliderRange * 0.5f, SliderRange * 0.5f, normalizedVerticalValue);

        if (_sliderHandleRectTransform != null)
        {
            _sliderHandleRectTransform.anchoredPosition = _defaultAnchoredPosition + new Vector2(horizontalOffset, verticalOffset);
            return;
        }

        _sliderHandle.localPosition = _defaultLocalPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
    }

    private int GetVerticalPositionCount()
    {
        return _verticalPositionCount > 0 ? _verticalPositionCount : _positionCount;
    }

    private int GetPressedPosition(int direction, int positionCount)
    {
        if (direction < 0)
        {
            return GetFirstPosition();
        }

        if (direction > 0)
        {
            return GetLastPosition(positionCount);
        }

        return GetCenteredPosition(positionCount);
    }

    public string GetCoordinatesText()
    {
        return $"{CurrentLatitude:F4}, {CurrentLongitude:F4}";
    }
}
