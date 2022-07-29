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

    public AudioClip MusicTransition;

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

    public void PlayMusic(AudioClip clip, AudioClip clipSecond)
    {
        musicSource.clip = clip;
        musicSource.Play();
        musicSourceSecond.clip = clipSecond;
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

    public IEnumerator ChangeMusic()
    {
        musicSource.Stop();
        PlaySound(MusicTransition);
        yield return new WaitForSeconds(3);
        musicSourceSecond.Play();
    }
}
