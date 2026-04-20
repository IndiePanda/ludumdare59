using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SenderView : MonoBehaviour
{
    [SerializeField] private DecoderPresenter _presenter;
    [SerializeField] private SymbolButtonElement[] _symbolButtonList;
    [SerializeField] private Element[] _elementList;
    [SerializeField] private TextMeshProUGUI _encryptedeText;
    [SerializeField] private TextMeshProUGUI _decryptedText;
    [SerializeField] private GameObject _decryptedPanel;
    [SerializeField] private Image[] _signalDetectorList;
    [SerializeField] private Sprite _activeSignalDetectorSprite;
    [SerializeField] private Sprite _inactiveSignalDetectorSprite;

    private TriggerPopupHandler _triggerPopupHandler;
    private SignalSystem _signalSystem;
    private CompletePanelView _completePanel;
    private int _activeElementIndex = -1;
    private string _encryptedSequence = string.Empty;
    private string _enteredSequence = string.Empty;
    private string _messageText = string.Empty;
    private bool _isCompleting;

    [Inject]
    private void Construct(TriggerPopupHandler triggerPopupHandler, SignalSystem signalSystem, CompletePanelView completePanel)
    {
        _triggerPopupHandler = triggerPopupHandler;
        _signalSystem = signalSystem;
        _completePanel = completePanel;
    }

    private void Start()
    {
        RefreshState();
        SetActiveElement(GetFirstAvailableElementIndex(GetActiveElementList()));

        if (_completePanel != null)
        {
            _completePanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isCompleting)
        {
            return;
        }

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

        if (_signalSystem != null && _signalSystem.TryGetCurrentSendingContent(out SignalSystem.SendingContent sendingContent))
        {
            _messageText = sendingContent.MessageText;
            SetAnswerButtonTexts(sendingContent.AnswerAText, sendingContent.AnswerBText);
        }
        else
        {
            _messageText = string.Empty;
            SetAnswerButtonTexts(string.Empty, string.Empty);
        }

        _enteredSequence = string.Empty;
        _encryptedeText.text = _encryptedSequence;
        _decryptedText.text = _enteredSequence;
        _decryptedPanel.SetActive(false);
        SetAllDetectorsInactive();
    }

    private void ActivateCurrentElement()
    {
        Element[] activeElementList = GetActiveElementList();
        if (activeElementList == null || _activeElementIndex < 0 || _activeElementIndex >= activeElementList.Length)
        {
            return;
        }

        Element activeElement = activeElementList[_activeElementIndex];
        if (activeElement is SymbolButtonElement symbolButtonElement)
        {
            SubmitSymbol(symbolButtonElement.Symbol);
            return;
        }

        if (activeElement is SendButtonElement)
        {
            if (_signalSystem.TryCompleteSending())
            {
                CompleteAndClose();
            }
        }
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
            SetDetectorState(_enteredSequence.Length - 1, true);
        }
        else
        {
            _enteredSequence = string.Empty;
            SetAllDetectorsInactive();
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

            _decryptedText.text = _messageText;
            SwitchActiveElementGroup();
        }
    }

    private void SetAnswerButtonTexts(string answerAText, string answerBText)
    {
        if (_elementList == null)
        {
            return;
        }

        int answerIndex = 0;

        for (int i = 0; i < _elementList.Length; i++)
        {
            if (_elementList[i] is not SendButtonElement sendButtonElement)
            {
                continue;
            }

            sendButtonElement.SetText(answerIndex == 0 ? answerAText : answerBText);
            answerIndex++;

            if (answerIndex >= 2)
            {
                return;
            }
        }
    }

    private void SetDetectorState(int detectorIndex, bool isActive)
    {
        if (_signalDetectorList == null || detectorIndex < 0 || detectorIndex >= _signalDetectorList.Length)
        {
            return;
        }

        Image detector = _signalDetectorList[detectorIndex];
        if (detector == null)
        {
            return;
        }

        detector.sprite = isActive ? _activeSignalDetectorSprite : _inactiveSignalDetectorSprite;
    }

    private void SetAllDetectorsInactive()
    {
        if (_signalDetectorList == null)
        {
            return;
        }

        for (int i = 0; i < _signalDetectorList.Length; i++)
        {
            SetDetectorState(i, false);
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
        Element[] activeElementList = GetActiveElementList();
        if (activeElementList == null || activeElementList.Length == 0)
        {
            return;
        }

        int currentIndex = _activeElementIndex >= 0 ? _activeElementIndex : GetFirstAvailableElementIndex(activeElementList);
        if (currentIndex < 0)
        {
            return;
        }

        int step = direction.x > 0f ? 1 : -1;
        int nextIndex = currentIndex;
        int checkedCount = 0;

        while (checkedCount < activeElementList.Length)
        {
            nextIndex = (nextIndex + step + activeElementList.Length) % activeElementList.Length;

            if (activeElementList[nextIndex] != null)
            {
                SetActiveElement(nextIndex);
                return;
            }

            checkedCount++;
        }
    }

    private void SetActiveElement(int elementIndex)
    {
        Element[] activeElementList = GetActiveElementList();
        if (activeElementList == null || elementIndex < 0 || elementIndex >= activeElementList.Length)
        {
            return;
        }

        if (_activeElementIndex >= 0 && _activeElementIndex < activeElementList.Length && activeElementList[_activeElementIndex] != null)
        {
            activeElementList[_activeElementIndex].SetActive(false);
        }

        _activeElementIndex = elementIndex;

        if (activeElementList[_activeElementIndex] != null)
        {
            activeElementList[_activeElementIndex].SetActive(true);
        }
    }

    private void SwitchActiveElementGroup()
    {
        Element[] previousElementList = _decryptedPanel != null && _decryptedPanel.activeSelf ? _symbolButtonList : _elementList;
        if (previousElementList != null && _activeElementIndex >= 0 && _activeElementIndex < previousElementList.Length && previousElementList[_activeElementIndex] != null)
        {
            previousElementList[_activeElementIndex].SetActive(false);
        }

        _activeElementIndex = -1;
        SetActiveElement(GetFirstAvailableElementIndex(GetActiveElementList()));
    }

    private Element[] GetActiveElementList()
    {
        return _decryptedPanel != null && _decryptedPanel.activeSelf ? _elementList : _symbolButtonList;
    }

    private int GetFirstAvailableElementIndex(Element[] elementList)
    {
        if (elementList == null)
        {
            return -1;
        }

        for (int i = 0; i < elementList.Length; i++)
        {
            if (elementList[i] != null)
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

    private void CompleteAndClose()
    {
        if (_isCompleting)
        {
            return;
        }

        _isCompleting = true;

        if (_completePanel != null)
        {
            _completePanel.gameObject.SetActive(true);
        }
        ClosePopup();
    }
}
