using UnityEngine;
using Zenject;

public class PressButtonHint : MonoBehaviour
{
    private StationManager _stationManager;

    [Inject]
    private void Construct(StationManager stationManager)
    {
        _stationManager = stationManager;
        _stationManager.ZoneEntered += Show;
        _stationManager.ZoneExited += Hide;
        Hide();
    }

    public void Show()
    {
        Debug.Log("Show");
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
