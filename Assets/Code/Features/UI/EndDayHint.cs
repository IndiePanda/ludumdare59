using System.Collections;
using UnityEngine;
using Zenject;

public class EndDayPanel : MonoBehaviour
{
    [SerializeField] private GameObject hint;
    [SerializeField] private GameObject blackScreen;

    private DaySystem _daySystem;
    private bool _isTransitioning;

    [Inject]
    public void Construct(DaySystem daySystem)
    {
        _daySystem = daySystem;
    }

    private void OnEnable()
    {
        if (_daySystem != null)
            _daySystem.NextDayAvailable += OnNextDayAvailable;
    }

    private void OnDisable()
    {
        if (_daySystem != null)
            _daySystem.NextDayAvailable -= OnNextDayAvailable;
    }

    private void OnNextDayAvailable()
    {
        if (hint != null)
            hint.SetActive(true);
    }

    private void Update()
    {
        if (_isTransitioning)
            return;

        if (hint != null && hint.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(PerformEndOfDayTransition());
        }
    }

    private IEnumerator PerformEndOfDayTransition()
    {
        _isTransitioning = true;

        if (hint != null)
            hint.SetActive(false);

        if (blackScreen != null)
            blackScreen.SetActive(true);

        yield return new WaitForSeconds(3f);

        _daySystem?.EndDay();

        if (blackScreen != null)
            blackScreen.SetActive(false);

        _isTransitioning = false;
    }
}
