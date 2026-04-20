using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SwitchElement : Element
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private Sprite _leftSprite;
    [SerializeField] private Sprite _rightSprite;

    private const int PositionCount = 2;

    private int _currentPosition;

    private SFXAudio _sfxAudio;

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

        _sfxAudio?.PlaySwitch();
    }

    [Inject]
    private void Construct(SFXAudio sfxAudio)
    {
        _sfxAudio = sfxAudio;
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
