using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SignalSystem : IDisposable
{
    public const float MaxLatitude = 90f;
    public const float MaxLongitude = 180f;

    private readonly DaySystem _daySystem;
    private readonly EnergySystem _energySystem;
    private readonly MessageSchedule _messageSchedule;
    private readonly Dictionary<int, MessageData> _plannedMessagesByMinute = new();
    private static readonly char[] SendingSymbols =
    {
        '\u2554', '\u2557', '\u255A', '\u255D', '\u2560', '\u2563', '\u2566', '\u2569', '\u256C', '\u2551', '\u2550'
    };

    private bool _hasPendingSignal;
    private bool _isSignalInProgress;
    private SignalTarget? _currentSignalTarget;
    private DecoderCombination? _currentDecoderCombination;
    private bool _hasPendingSending;
    private string _currentSendingSequence;
    private MessageData _currentMessageData;

    public bool HasPendingSignal => _hasPendingSignal;
    public bool HasPendingDecoding => _currentDecoderCombination.HasValue;
    public bool HasPendingSending => _hasPendingSending;

    public event Action SignalAvailabilityChanged;

    public readonly struct SignalTarget
    {
        public SignalTarget(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public float Latitude { get; }
        public float Longitude { get; }
    }

    public readonly struct DecoderCombination
    {
        public DecoderCombination(int signalType, int signalValue, int wavelength, int waveHeight)
        {
            SignalType = signalType;
            SignalValue = signalValue;
            Wavelength = wavelength;
            WaveHeight = waveHeight;
        }

        public int SignalType { get; }
        public int SignalValue { get; }
        public int Wavelength { get; }
        public int WaveHeight { get; }
    }

    public readonly struct SendingContent
    {
        public SendingContent(string messageText, string answerAText, string answerBText)
        {
            MessageText = messageText;
            AnswerAText = answerAText;
            AnswerBText = answerBText;
        }

        public string MessageText { get; }
        public string AnswerAText { get; }
        public string AnswerBText { get; }
    }

    public SignalSystem(DaySystem daySystem, EnergySystem energySystem, MessageSchedule messageSchedule)
    {
        _daySystem = daySystem;
        _energySystem = energySystem;
        _messageSchedule = messageSchedule;
        _daySystem.DayChanged += OnDayChanged;
        _daySystem.MinuteChanged += OnMinuteChanged;

        ScheduleSignalsForDay(_daySystem.CurrentDay);
        OnMinuteChanged(_daySystem.CurrentDay, _daySystem.CurrentMinute);
    }

    public bool TryStartSignalSearch()
    {
        if (_isSignalInProgress)
        {
            return false;
        }

        if (!_hasPendingSignal)
        {
            return false;
        }

        bool hadPendingSignal = HasPendingSignal;
        _hasPendingSignal = false;
        _isSignalInProgress = true;
        NotifySignalAvailabilityChanged();
        return true;
    }

    public bool TryReceiveSignal()
    {
        if (!_currentSignalTarget.HasValue || HasPendingDecoding || _hasPendingSending)
        {
            return false;
        }

        _hasPendingSignal = false;
        _isSignalInProgress = false;
        _currentSignalTarget = null;
        _currentDecoderCombination = CreateRandomDecoderCombination();
        _energySystem.TrySpendEnergy(1);
        NotifySignalAvailabilityChanged();
        return true;
    }

    public bool TryCompleteDecoding()
    {
        if (!HasPendingDecoding)
        {
            return false;
        }

        _currentDecoderCombination = null;
        _currentSendingSequence = CreateRandomSendingSequence();
        _hasPendingSending = true;
        NotifySignalAvailabilityChanged();
        return true;
    }

    public bool TryCompleteSending()
    {
        if (!_hasPendingSending)
        {
            return false;
        }

        _hasPendingSending = false;
        _currentSendingSequence = null;
        _currentMessageData = null;
        NotifySignalAvailabilityChanged();
        return true;
    }

    private void OnDayChanged(int day)
    {
        _isSignalInProgress = false;
        _currentSignalTarget = null;
        _currentDecoderCombination = null;
        _hasPendingSignal = false;
        _hasPendingSending = false;
        _currentSendingSequence = null;
        _currentMessageData = null;
        NotifySignalAvailabilityChanged();
        ScheduleSignalsForDay(day);
    }

    private void OnMinuteChanged(int day, int minute)
    {
        if (_plannedMessagesByMinute.TryGetValue(minute, out MessageData messageData))
        {
            if (_isSignalInProgress || _hasPendingSignal || HasPendingDecoding || _hasPendingSending)
            {
                return;
            }

            SignalTarget signalTarget = CreateRandomSignalTarget();
            _currentSignalTarget = signalTarget;
            _currentMessageData = messageData;
            _hasPendingSignal = true;
            NotifySignalAvailabilityChanged();
            //Debug.Log($"Signal created on day {day} at {FormatTime(minute)}. Target: {signalTarget.Latitude:F3}, {signalTarget.Longitude:F3}. Message: {messageData.messageText}");
        }
    }

    private void ScheduleSignalsForDay(int day)
    {
        _plannedMessagesByMinute.Clear();

        if (_messageSchedule == null || _messageSchedule.days == null)
        {
            return;
        }

        for (int i = 0; i < _messageSchedule.days.Count; i++)
        {
            DayData dayData = _messageSchedule.days[i];
            if (dayData == null || dayData.dayIndex != day || dayData.messages == null)
            {
                continue;
            }

            for (int j = 0; j < dayData.messages.Count; j++)
            {
                MessageData messageData = dayData.messages[j];
                if (messageData == null)
                {
                    continue;
                }

                int minute = ConvertScheduleTimeToMinute(messageData.time);
                _plannedMessagesByMinute[minute] = messageData;
            }

            break;
        }
    }

    private void NotifySignalAvailabilityChanged()
    {
        SignalAvailabilityChanged?.Invoke();
    }

    private static string FormatTime(int minute)
    {
        return DaySystem.FormatTime(minute);
    }

    public bool TryGetCurrentSignalTarget(out SignalTarget signalTarget)
    {
        if (_currentSignalTarget.HasValue)
        {
            signalTarget = _currentSignalTarget.Value;
            return true;
        }

        signalTarget = default;
        return false;
    }

    public bool TryGetCurrentDecoderCombination(out DecoderCombination decoderCombination)
    {
        if (_currentDecoderCombination.HasValue)
        {
            decoderCombination = _currentDecoderCombination.Value;
            return true;
        }

        decoderCombination = default;
        return false;
    }

    public bool TryGetCurrentSendingSequence(out string sendingSequence)
    {
        if (!string.IsNullOrEmpty(_currentSendingSequence))
        {
            sendingSequence = _currentSendingSequence;
            return true;
        }

        sendingSequence = null;
        return false;
    }

    public bool TryGetCurrentSendingContent(out SendingContent sendingContent)
    {
        if (_currentMessageData != null)
        {
            sendingContent = new SendingContent(
                _currentMessageData.messageText,
                _currentMessageData.answerA != null ? _currentMessageData.answerA.text : string.Empty,
                _currentMessageData.answerB != null ? _currentMessageData.answerB.text : string.Empty);
            return true;
        }

        sendingContent = default;
        return false;
    }

    private static SignalTarget CreateRandomSignalTarget()
    {
        float latitude = UnityEngine.Random.Range(0f, MaxLatitude);
        float longitude = UnityEngine.Random.Range(0f, MaxLongitude);
        return new SignalTarget(latitude, longitude);
    }

    private static DecoderCombination CreateRandomDecoderCombination()
    {
        return new DecoderCombination(
            UnityEngine.Random.Range(0, 4),
            (UnityEngine.Random.Range(1, 5)) * 16,
            UnityEngine.Random.Range(0, 4),
            UnityEngine.Random.Range(0, 4));
    }

    private static string CreateRandomSendingSequence()
    {
        List<char> availableSymbols = new(SendingSymbols);
        var sequenceBuilder = new StringBuilder(8);

        for (int i = 0; i < 8; i++)
        {
            int symbolIndex = UnityEngine.Random.Range(0, availableSymbols.Count);
            sequenceBuilder.Append(availableSymbols[symbolIndex]);
            availableSymbols.RemoveAt(symbolIndex);
        }

        return sequenceBuilder.ToString();
    }

    private static int ConvertScheduleTimeToMinute(float time)
    {
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.RoundToInt((time - hours) * 100f);
        int totalMinutes = (hours - DaySystem.StartHour) * 60 + minutes;
        return Mathf.Clamp(totalMinutes, 0, DaySystem.DayDurationMinutes);
    }

    public void Dispose()
    {
        _daySystem.DayChanged -= OnDayChanged;
        _daySystem.MinuteChanged -= OnMinuteChanged;
        SignalAvailabilityChanged = null;
    }
}
