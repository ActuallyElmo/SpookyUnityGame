using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float defaultMaxRange = 50f; // Distance where sound becomes silent

    void Awake()
    {
        Instance = this;
    }

    // Added 'maxRange' parameter with a default value
    public void PlaySoundEffect(AudioClip audioClip, GameObject targetObject, float volume = 1f, AudioMixerGroup audioMixerGroup = null, float maxRange = -1f)
    {
        // Use default range if none is provided
        float actualRange = maxRange > 0 ? maxRange : defaultMaxRange;

        StartCoroutine(PlaySound(audioClip, targetObject, volume, audioMixerGroup, actualRange));
    }

    private IEnumerator PlaySound(AudioClip audioClip, GameObject targetObject, float baseVolume, AudioMixerGroup audioMixerGroup, float maxRange)
    {
        // 1. Check if Player exists to calculate distance
        float distanceFactor = 1f;
        
        if (PlayerSingleton.instance != null && targetObject != null)
        {
            float distance = Vector3.Distance(targetObject.transform.position, PlayerSingleton.instance.transform.position);
            
            // Linear Rolloff Math: 1 at 0 distance, 0 at maxRange
            // Mathf.Clamp01 prevents volume from going negative or above 1
            distanceFactor = Mathf.Clamp01(1 - (distance / maxRange));
        }

        // If the player is too far, we can skip playing entirely to save performance
        if(distanceFactor <= 0) yield break;

        AudioSource audioSource = targetObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;

        // 2. Apply the distance scaling
        audioSource.volume = baseVolume * distanceFactor;

        // Optional: Set Spatial Blend to 0 (2D) so Unity doesn't try to double-calculate physics distance
        // Set to 1 (3D) if you still want left/right panning, but ensure Unity's built-in Min/Max distance doesn't conflict
        audioSource.spatialBlend = 0f; 

        if (audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }

        audioSource.Play();

        // Wait for clip to finish
        yield return new WaitForSeconds(audioClip.length);

        // Cleanup
        if(audioSource != null)
        {
            Destroy(audioSource);
        }
    }
}