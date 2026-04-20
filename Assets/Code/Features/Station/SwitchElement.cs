using UnityEngine;
using UnityEngine.UI;

public class SwitchElement : Element
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite _leftSprite;
    [SerializeField] private Sprite _rightSprite;

    private const int PositionCount = 2;

    private int _currentPosition;

    public int CurrentPosition => _currentPosition;

    protected override void CacheReferences()
    {
        _targetImage ??= GetComponentInChildren<Image>();
    }

    protected override void SetDefaultValue()
    {
        _currentPosition = GetFirstPosition();
        ApplyVisualState();
    }

    protected override void ChangeValueInternal(Vector2Int direction)
    {
        if (!TryChangePosition(ref _currentPosition, PositionCount, direction.x))
        {
            return;
        }

        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (_targetImage == null)
        {
            return;
        }

        _targetImage.sprite = _currentPosition == GetFirstPosition() ? _leftSprite : _rightSprite;
    }
}
