using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMusic : MonoBehaviour
{
    [Header("Paladin Boss")]
    public AudioClip paladinBossMusic1;       // for phase 1
    public AudioClip paladinBossMusic2;       // for phase 2

    // references
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    public void SetAudioClip(AudioClip clip, bool loop, float volume)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
    }

    public void PlayCurrentClip() {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.Play();
    }
}
