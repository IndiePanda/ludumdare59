using UnityEngine;
using UnityEngine.UI;

public abstract class Element : MonoBehaviour
{
    [SerializeField] private bool _isActive;

    private bool _isInitialized;
    [SerializeField] private Image _image;

    private static readonly Color ActiveColor = Color.yellow;
    private static readonly Color InactiveColor = Color.white;

    protected virtual void Awake()
    {
        Initialize();
        SetDefaultValue();
        ApplyActiveState();
    }

    public virtual bool UsesHoldInput => false;

    public void SetActive(bool isActive)
    {
        Initialize();
        _isActive = isActive;
        ApplyActiveState();
    }

    public void ChangeValue(int direction)
    {
        ChangeValue(new Vector2Int(direction, 0));
    }

    public void ChangeValue(Vector2Int direction)
    {
        ApplyInput(direction);
    }

    public virtual void ApplyInput(Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
        {
            return;
        }

        Initialize();
        ChangeValueInternal(direction);
    }

    public virtual void ReleaseInput()
    {
    }

    protected abstract void SetDefaultValue();

    protected abstract void ChangeValueInternal(Vector2Int direction);

    protected virtual void CacheReferences()
    {
    }

    protected void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        CacheReferences();

        if (_image == null)
            _image = GetComponentInChildren<Image>();

        _isInitialized = true;
    }

    protected static bool TryChangePosition(ref int currentPosition, int positionCount, int direction)
    {
        if (direction == 0)
        {
            return false;
        }

        int maxPosition = Mathf.Max(1, positionCount);
        int nextPosition = Mathf.Clamp(currentPosition + direction, 1, maxPosition);
        if (nextPosition == currentPosition)
        {
            return false;
        }

        currentPosition = nextPosition;
        return true;
    }

    protected static float GetNormalizedValue(int currentPosition, int positionCount)
    {
        int maxPosition = Mathf.Max(1, positionCount);
        if (maxPosition == 1)
        {
            return 0f;
        }

        return (currentPosition - 1f) / (maxPosition - 1f);
    }

    protected static int GetFirstPosition()
    {
        return 1;
    }

    protected static int GetCenteredPosition(int positionCount)
    {
        int maxPosition = Mathf.Max(1, positionCount);
        return (maxPosition + 1) / 2;
    }

    protected static int GetLastPosition(int positionCount)
    {
        return Mathf.Max(1, positionCount);
    }

    private void ApplyActiveState()
    {
        _image.color = _isActive ? ActiveColor : InactiveColor;
    }
}