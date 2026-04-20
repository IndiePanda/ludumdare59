using UnityEngine;
using Zenject;

public class TutorialSystem : IInitializable, ITickable
{
    private readonly TutorialView _tutorialView;
    private readonly TutorialTextBase _tutorialTextBase;
    private readonly DaySystem _daySystem;

    private int _currentSlideIndex;
    private int _ignoreInputUntilFrame;
    private float _timeScaleBeforeTutorial = 1f;

    public TutorialSystem(TutorialView tutorialView, TutorialTextBase tutorialTextBase, DaySystem daySystem)
    {
        _tutorialView = tutorialView;
        _tutorialTextBase = tutorialTextBase;
        _daySystem = daySystem;
    }

    public bool IsActive { get; private set; }

    public void Initialize()
    {
        _tutorialView.Hide();
    }

    public void Tick()
    {
        if (!IsActive || Time.frameCount <= _ignoreInputUntilFrame)
        {
            return;
        }

        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        _currentSlideIndex++;

        if (_currentSlideIndex >= GetSlideCount())
        {
            CompleteTutorial();
            return;
        }

        _tutorialView.SetText(_tutorialTextBase.Text[_currentSlideIndex]);
    }

    public void TryShowOnGameStart()
    {
        if (_daySystem.CurrentDay != 1)
        {
            return;
        }

        if (GetSlideCount() <= 0)
        {
            CompleteTutorial();
            return;
        }

        _timeScaleBeforeTutorial = Time.timeScale;
        Time.timeScale = 0f;
        IsActive = true;
        _currentSlideIndex = 0;
        _ignoreInputUntilFrame = Time.frameCount;

        _tutorialView.Show();
        _tutorialView.SetText(_tutorialTextBase.Text[_currentSlideIndex]);
    }

    private int GetSlideCount()
    {
        return _tutorialTextBase?.Text?.Length ?? 0;
    }

    private void CompleteTutorial()
    {
        IsActive = false;
        Time.timeScale = _timeScaleBeforeTutorial;
        _tutorialView.Hide();
    }
}
