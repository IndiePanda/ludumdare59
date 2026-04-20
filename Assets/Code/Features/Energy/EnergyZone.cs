using System;
using UnityEngine;
using Zenject;

public class EnergyZone : MonoBehaviour
{
    [SerializeField] private string _ID;
    [SerializeField] private float _restoreCooldown;

    private float _nextRestoreTime;
    private bool _isCharacterInsideZone;
    private bool _isAvailable = true;
    private SFXAudio _sfxAudio;

    public bool IsAvailable => _isAvailable;

    public event Action ZoneEntered;
    public event Action ZoneExited;
    public event Action EnergyRestoreRequested;
    public event Action<bool> AvailabilityChanged;

    [Inject]
    public void Construct(SFXAudio sfxAudio)
    {
        _sfxAudio = sfxAudio;
    }

    private void Update()
    {
        if (_isAvailable || Time.time < _nextRestoreTime)
        {
            return;
        }

        SetAvailability(true);

        if (_isCharacterInsideZone)
        {
            ZoneEntered?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = true;

        if (!_isAvailable)
        {
            return;
        }

        ZoneEntered?.Invoke();


        if (_restoreCooldown > 0f)
        {
            SetAvailability(false);
        }
    }

    public void TryCollectZone()
    {
        _nextRestoreTime = Time.time + _restoreCooldown;
        EnergyRestoreRequested?.Invoke();
        SetAvailability(false);
        _sfxAudio?.PlayLoot();
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<CharacterMovement>() == null)
        {
            return;
        }

        _isCharacterInsideZone = false;
        ZoneExited?.Invoke();
    }

    private void SetAvailability(bool isAvailable)
    {
        if (_isAvailable == isAvailable)
        {
            return;
        }

        _isAvailable = isAvailable;
        AvailabilityChanged?.Invoke(_isAvailable);
        transform.GetChild(0).gameObject.SetActive(isAvailable);
    }
}
