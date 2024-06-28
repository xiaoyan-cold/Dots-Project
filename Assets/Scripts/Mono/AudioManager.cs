using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource audioSource;
    public AudioClip shootAudioClip;
    public AudioClip hitAudioClip;

    public float playHitAudioInterval = 0.2f;
    // public float playHitAudioValidTime = 0.1f;
    private float lastPlayerHitAudioTime;

    private void Awake()
    {
        instance = this;
    }
    public void PlayShootAudio()
    {
        audioSource.PlayOneShot(shootAudioClip);
    }

    public void PlayHitAudio()
    {
        audioSource.PlayOneShot(hitAudioClip);
    }

    private void Update()
    {
        if (SharedData.gameSharedData.Data.playHitAudio && Time.time - lastPlayerHitAudioTime > playHitAudioInterval && Time.time - SharedData.gameSharedData.Data.playHitAudioTime < Time.deltaTime)
        {
            lastPlayerHitAudioTime = Time.time;
            PlayHitAudio();
            SharedData.gameSharedData.Data.playHitAudio = false;
        }
    }
}
