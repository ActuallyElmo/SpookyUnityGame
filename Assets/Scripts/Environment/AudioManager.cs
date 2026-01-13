using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void PlaySoundEffect(AudioClip audioClip, GameObject targetObject, float volume = 1f, AudioMixerGroup audioMixerGroup = null)
    {
        StartCoroutine(PlaySound(audioClip, targetObject, volume, audioMixerGroup));
    }

    private IEnumerator PlaySound(AudioClip audioClip, GameObject targetObject, float volume, AudioMixerGroup audioMixerGroup)
    {
        AudioSource audioSource = targetObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        if(audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }
        audioSource.Play();

        yield return new WaitForSeconds(audioSource.clip.length);

        Destroy(audioSource);
    }
}
