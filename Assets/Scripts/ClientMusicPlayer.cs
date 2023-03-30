using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ClientMusicPlayer : Singleton<ClientMusicPlayer>
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    public override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudioClip()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
