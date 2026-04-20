using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SignalSystem : IDisposable
{
    private const int SignalsPerDay = 3;
    public const float MaxLatitude = 90f;
    public const float MaxLongitude = 180f;

    private readonly DaySystem _daySystem;
    private readonly EnergySystem _energySystem;
    private readonly HashSet<int> _plannedSignalMinutes = new();
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

    public SignalSystem(DaySystem daySystem, EnergySystem energySystem)
    {
        _daySystem = daySystem;
        _energySystem = energySystem;
        _daySystem.DayChanged += OnDayChanged;
        _daySystem.MinuteChanged += OnMinuteChanged;

        ScheduleSignalsForDay(_daySystem.CurrentDay);
        OnMinuteChanged(_daySystem.CurrentDay, _daySystem.CurrentMinute);
    }

    public bool TryStartSignalSearch()
    {
        if (_isSignalInProgress)
        {
            Debug.Log("Signal search already started.");
            return false;
        }

        if (!_hasPendingSignal)
        {
            Debug.Log("No signal available for search.");
            return false;
        }

        bool hadPendingSignal = HasPendingSignal;
        _hasPendingSignal = false;
        _isSignalInProgress = true;
        NotifySignalAvailabilityChanged();
        Debug.Log("Player started searching for signal in SatelliteZone.");
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
        NotifySignalAvailabilityChanged();
        ScheduleSignalsForDay(day);
    }

    private void OnMinuteChanged(int day, int minute)
    {
        bool signalCreatedThisMinute = false;

        if (_plannedSignalMinutes.Contains(minute))
        {
            if (_isSignalInProgress || _hasPendingSignal || HasPendingDecoding || _hasPendingSending)
            {
                return;
            }

            SignalTarget signalTarget = CreateRandomSignalTarget();
            _currentSignalTarget = signalTarget;
            _hasPendingSignal = true;
            NotifySignalAvailabilityChanged();
            signalCreatedThisMinute = true;
            Debug.Log($"Signal created on day {day} at {FormatTime(minute)}. Target: {signalTarget.Latitude:F3}, {signalTarget.Longitude:F3}");
        }

        if (!signalCreatedThisMinute && _hasPendingSignal)
        {
            //Debug.Log($"Signal is waiting for accept. Day {day}, time {FormatTime(minute)}. Pending signals: {_pendingSignalsCount}.");
        }
    }

    private void ScheduleSignalsForDay(int day)
    {
        _plannedSignalMinutes.Clear();

        while (_plannedSignalMinutes.Count < SignalsPerDay)
        {
            var randomMinute = UnityEngine.Random.Range(0, DaySystem.DayDurationMinutes);
            Debug.Log(randomMinute);
            _plannedSignalMinutes.Add(randomMinute);
        }

        Debug.Log($"Signal schedule prepared for day {day}.");
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

    public void Dispose()
    {
        _daySystem.DayChanged -= OnDayChanged;
        _daySystem.MinuteChanged -= OnMinuteChanged;
        SignalAvailabilityChanged = null;
    }
}
