using UnityEngine;

public class SFXAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _switchClip;

    public void PlaySwitch()
    {
        if (_audioSource == null || _switchClip == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_switchClip);
    }
}
