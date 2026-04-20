using TMPro;
using UnityEngine;
using Zenject;

public class SymbolButtonElement : Element
{
    [SerializeField] private string _symbol;
    [SerializeField] private TextMeshProUGUI _label;

    public char Symbol => string.IsNullOrEmpty(_symbol) ? default : _symbol[0];

    private SFXAudio _sfxAudio;

    protected override void CacheReferences()
    {
        _label ??= GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void SetDefaultValue()
    {
        if (_label != null)
        {
            _label.text = _symbol;
        }
    }

    protected override void ChangeValueInternal(Vector2Int direction)
    {
    }

    public override void ApplyInput(Vector2Int direction)
    {
        // Treat any non-zero input as a button press
        if (direction == Vector2Int.zero)
        {
            return;
        }

        _sfxAudio?.PlaySwitch();
    }

    [Inject]
    private void Construct(SFXAudio sfxAudio)
    {
        _sfxAudio = sfxAudio;
    }
}
