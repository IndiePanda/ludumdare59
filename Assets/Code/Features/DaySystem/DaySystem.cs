using System;
using UnityEngine;
using Zenject;

public class DaySystem : ITickable, IDisposable
{
    public const int StartHour = 8;
    public const int EndHour = 23;
    public const int DayDurationMinutes = (EndHour - StartHour) * 60;

    private const float SecondsPerGameMinute = 0.1f;

    private int _currentDay = 1;
    private int _currentMinute;
    private float _minuteTimer;
    private bool _isNextDayAvailable;

    public int CurrentDay => _currentDay;
    public int CurrentMinute => _currentMinute;
    public bool IsNextDayAvailable => _isNextDayAvailable;
    public string CurrentTime => FormatTime(_currentMinute);

    public event Action<int> DayChanged;
    public event Action<int, int> MinuteChanged;
    public event Action NextDayAvailable;

    public void Tick()
    {
        if (_currentMinute >= DayDurationMinutes)
        {
            return;
        }

        _minuteTimer += Time.deltaTime;

        while (_minuteTimer >= SecondsPerGameMinute && _currentMinute < DayDurationMinutes)
        {
            _minuteTimer -= SecondsPerGameMinute;
            AdvanceMinute();
        }
    }

    public void EndDay()
    {
        _minuteTimer = 0f;
        _currentMinute = 0;
        _isNextDayAvailable = false;
        _currentDay++;
        DayChanged?.Invoke(_currentDay);
        MinuteChanged?.Invoke(_currentDay, _currentMinute);
    }

    private void AdvanceMinute()
    {
        if (_currentMinute >= DayDurationMinutes)
        {
            return;
        }
        
        _currentMinute++;
        MinuteChanged?.Invoke(_currentDay, _currentMinute);

        if (_currentMinute >= DayDurationMinutes && !_isNextDayAvailable)
        {
            _isNextDayAvailable = true;
            NextDayAvailable?.Invoke();
        }
    }

    public static string FormatTime(int minute)
    {
        int totalMinutes = StartHour * 60 + minute;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        return $"{hours:00}:{minutes:00}";
    }

    public void Dispose()
    {
        DayChanged = null;
        MinuteChanged = null;
        NextDayAvailable = null;
    }
}
