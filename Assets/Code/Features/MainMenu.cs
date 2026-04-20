using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private float _scrollSpeed = 0.5f;

    private Rect _uvRect;
    private Button[] _buttons;

    private void Awake()
    {
        _uvRect = _rawImage.uvRect;
        _buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in _buttons)
        {
            button.onClick.AddListener(HideMenu);
        }

        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        if (_buttons == null)
        {
            return;
        }

        foreach (Button button in _buttons)
        {
            button.onClick.RemoveListener(HideMenu);
        }
    }

    private void Update()
    {
        if (IsAnyKeyboardKeyDown())
        {
            HideMenu();
            return;
        }

        _uvRect.x += _scrollSpeed * Time.unscaledDeltaTime;
        _rawImage.uvRect = _uvRect;
    }

    private void HideMenu()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private static bool IsAnyKeyboardKeyDown()
    {
        foreach (KeyCode keyCode in (KeyCode[])System.Enum.GetValues(typeof(KeyCode)))
        {
            if (keyCode == KeyCode.None)
            {
                continue;
            }

            if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
            {
                continue;
            }

            if (keyCode.ToString().StartsWith("Joystick"))
            {
                continue;
            }

            if (Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }
}
