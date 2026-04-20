using TMPro;
using UnityEngine;

public class SymbolButtonElement : Element
{
    [SerializeField] private string _symbol;
    [SerializeField] private TextMeshProUGUI _label;

    public char Symbol => string.IsNullOrEmpty(_symbol) ? default : _symbol[0];

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
}
