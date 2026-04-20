using TMPro;
using UnityEngine;

public class TutorialView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        if (_text == null)
        {
            return;
        }

        _text.text = text;
    }
}
