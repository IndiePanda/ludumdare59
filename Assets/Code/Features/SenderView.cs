using TMPro;
using UnityEngine;
using Zenject;

public class SenderView : MonoBehaviour
{
    [SerializeField] private DecoderPresenter _presenter;
    [SerializeField] private Element[] _elementList;
    [SerializeField] private TextMeshProUGUI _encryptedeText;
    [SerializeField] private TextMeshProUGUI _decryptedText;
    [SerializeField] private GameObject _decryptedPanel;

    private TriggerPopupHandler _triggerPopupHandler;
    private SignalSystem _signalSystem;
    private int _activeElementIndex = -1;
    private string _encryptedSequence = string.Empty;
    private string _enteredSequence = string.Empty;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
    }

    private void Start()
    {
        SetActiveElement(GetFirstAvailableElementIndex());
        RefreshState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClosePopup();
            return;
        }

        Vector2 navigationDirection = GetNavigationDirection();
        if (navigationDirection != Vector2.zero)
        {
            MoveSelection(navigationDirection);
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ActivateCurrentElement();
        }
    }

    private void RefreshState()
    {
        if (_signalSystem != null && _signalSystem.TryGetCurrentSendingSequence(out string encryptedSequence))
        {
            _encryptedSequence = encryptedSequence;
        }
        else
        {
            _encryptedSequence = string.Empty;
        }

        _enteredSequence = string.Empty;
        _encryptedeText.text = _encryptedSequence;
        _decryptedText.text = _enteredSequence;
        _decryptedPanel.SetActive(false);
    }

    private void ActivateCurrentElement()
    {
        if (_activeElementIndex < 0 || _activeElementIndex >= _elementList.Length)
        {
            return;
        }

        Element activeElement = _elementList[_activeElementIndex];
        if (activeElement is not SymbolButtonElement symbolButtonElement)
        {
            return;
        }

        SubmitSymbol(symbolButtonElement.Symbol);
    }

    private void SubmitSymbol(char selectedSymbol)
    {
        if (string.IsNullOrEmpty(_encryptedSequence) || _enteredSequence.Length >= _encryptedSequence.Length)
        {
            return;
        }

        char expectedSymbol = _encryptedSequence[_enteredSequence.Length];

        if (selectedSymbol == expectedSymbol)
        {
            _enteredSequence += selectedSymbol;
        }
        else
        {
            _enteredSequence = string.Empty;
        }

        if (_decryptedText != null)
        {
            _decryptedText.text = _enteredSequence;
        }

        if (_enteredSequence.Length == _encryptedSequence.Length)
        {
            if (_decryptedPanel != null)
            {
                _decryptedPanel.SetActive(true);
            }

            _signalSystem.TryCompleteSending();
        }
    }

    private Vector2 GetNavigationDirection()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            return Vector2.left;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            return Vector2.right;
        }

        return Vector2.zero;
    }

    private void MoveSelection(Vector2 direction)
    {
        if (_elementList == null || _elementList.Length == 0)
        {
            return;
        }

        int currentIndex = _activeElementIndex >= 0 ? _activeElementIndex : GetFirstAvailableElementIndex();
        if (currentIndex < 0)
        {
            return;
        }

        int step = direction.x > 0f ? 1 : -1;
        int nextIndex = currentIndex;
        int checkedCount = 0;

        while (checkedCount < _elementList.Length)
        {
            nextIndex = (nextIndex + step + _elementList.Length) % _elementList.Length;

            if (_elementList[nextIndex] != null)
            {
                SetActiveElement(nextIndex);
                return;
            }

            checkedCount++;
        }
    }

    private void SetActiveElement(int elementIndex)
    {
        if (_elementList == null || elementIndex < 0 || elementIndex >= _elementList.Length)
        {
            return;
        }

        if (_activeElementIndex >= 0 && _activeElementIndex < _elementList.Length && _elementList[_activeElementIndex] != null)
        {
            _elementList[_activeElementIndex].SetActive(false);
        }

        _activeElementIndex = elementIndex;

        if (_elementList[_activeElementIndex] != null)
        {
            _elementList[_activeElementIndex].SetActive(true);
        }
    }

    private int GetFirstAvailableElementIndex()
    {
        if (_elementList == null)
        {
            return -1;
        }

        for (int i = 0; i < _elementList.Length; i++)
        {
            if (_elementList[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private void ClosePopup()
    {
        _triggerPopupHandler.CloseCurrent();
        Destroy(gameObject);
    }
}
