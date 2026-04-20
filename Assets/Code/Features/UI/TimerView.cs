using System;
using TMPro;
using UnityEngine;
using Zenject;

public class TimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _day;

    public void UpdateUI(int day, string time)
    {
        _time.text = time;
        _day.SetText("Day {0}", day);
    }
}

public class TimerPresenter : IInitializable, IDisposable
{
    private readonly TimerView _timerView;
    private readonly DaySystem _daySystem;

    public TimerPresenter(
        TimerView timerView,
        DaySystem daySystem
        )
    {
        _timerView = timerView;
        _daySystem = daySystem;
    }

    public void Initialize()
    {
        _daySystem.MinuteChanged += OnMinuteChanged;
        _timerView.UpdateUI(_daySystem.CurrentDay, _daySystem.CurrentTime);
    }

    public void Dispose()
    {
        _daySystem.MinuteChanged -= OnMinuteChanged;
    }

    private void OnMinuteChanged(int day, int minute)
    {
        _timerView.UpdateUI(day, DaySystem.FormatTime(minute));
    }
}
