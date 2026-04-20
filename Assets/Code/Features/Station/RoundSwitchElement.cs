using UnityEngine;
using Zenject;

public class RoundSwitchElement : Element
{
    private const float SwitchAngleRange = 180f;

    [SerializeField] private int _positionCount;
    [SerializeField] private Transform _switchTarget;

    private int _currentPosition;
    private Quaternion _defaultRotation;
    private SFXAudio _sfxAudio;

    public int CurrentPosition => _currentPosition;

    protected override void CacheReferences()
    {
        _switchTarget ??= transform;

        if (_switchTarget != null)
        {
            _defaultRotation = _switchTarget.localRotation;
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
        if (_switchTarget == null)
        {
            return;
        }

        float normalizedValue = GetNormalizedValue(_currentPosition, _positionCount);
        float angle = Mathf.Lerp(SwitchAngleRange * 0.5f, -SwitchAngleRange * 0.5f, normalizedValue);
        _switchTarget.localRotation = _defaultRotation * Quaternion.Euler(0f, 0f, angle);
    }
}
