using System;
using UnityEngine;

public class SFXAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _switchClip;
    [SerializeField] private AudioClip _lootClip;

    public void PlaySwitch()
    {
        _audioSource.PlayOneShot(_switchClip);
    }

    public void PlayLoot()
    {
        _audioSource.PlayOneShot(_lootClip);
    }
}
