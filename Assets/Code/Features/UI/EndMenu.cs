using UnityEngine;
using Zenject;

public class EndMenu : MonoBehaviour
{
    [SerializeField] private GameObject endMenuPanel;

    private DaySystem _daySystem;

    [Inject]
    public void Construct(DaySystem daySystem)
    {
        _daySystem = daySystem;
    }

    private void OnEnable()
    {
        if (_daySystem != null)
            _daySystem.DayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        if (_daySystem != null)
            _daySystem.DayChanged -= OnDayChanged;
    }

    private void OnDayChanged(int day)
    {
        if (day > 6)
        {
            if (endMenuPanel != null)
                endMenuPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }
}
