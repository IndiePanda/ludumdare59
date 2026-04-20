using TMPro;
using UnityEngine;
using Zenject;

public class SendButtonElement : Element
{
    [SerializeField] private string _text;
    [SerializeField] private TextMeshProUGUI _label;

    public string Text => _text;

    protected override void CacheReferences()
    {
        _label ??= GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void SetDefaultValue()
    {
        ApplyText();
    }

    protected override void ChangeValueInternal(Vector2Int direction)
    {
    }

    private SFXAudio _sfxAudio;

    public override void ApplyInput(Vector2Int direction)
    {
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

    public void SetText(string text)
    {
        _text = text;
        ApplyText();
    }

    private void ApplyText()
    {
        if (_label != null)
        {
            _label.text = _text;
        }
    }
}
