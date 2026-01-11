using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.ComponentModel.Design.Serialization;

public class PlayerSoundManager : MonoBehaviour
{
    private const float footstepSoundDelay = 0.5f;
    private FirstPersonController firstPersonController;

    [Range(0f, 1f)]
    public float footstepSoundVolume;

    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioMixerGroup footstepsMixerGroup;

    bool isPlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        Vector3 currentMovement = firstPersonController.currentMovement;
        if((Mathf.Abs(currentMovement.x) >= 2f || Mathf.Abs(currentMovement.z) >= 2f) && !isPlaying)
        {
            StartCoroutine(PlayFootsteps());
        }
    }

    private IEnumerator PlayFootsteps()
    {
        isPlaying = true;
        AudioManager.Instance.PlaySoundEffect(footstepSound, gameObject, footstepSoundVolume, footstepsMixerGroup);

        yield return new WaitForSeconds(footstepSoundDelay);

        isPlaying = false;
    }
}
