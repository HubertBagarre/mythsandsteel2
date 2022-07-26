using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource musicSourceSecond;
    public AudioSource effectSource;
    public AudioSource effectSourceSecond;

    public static SoundManager Instance = null;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip, bool second = false)
    {
        if (!second)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
        else
        {
            musicSourceSecond.clip = clip;
            musicSourceSecond.Play();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (!effectSource.isPlaying)
        {
            effectSource.clip = clip;
            effectSource.Play();
        }
        else
        {
            effectSourceSecond.clip = clip;
            effectSourceSecond.Play();
        }
    }
}
