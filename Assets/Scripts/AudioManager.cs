using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource audioSourcePrefab;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void PlayAudio(AudioClip audio, Vector3 position, float volume)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, position, Quaternion.identity, transform);
        audioSource.clip = audio;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, audioSource.clip.length);
    }
    public void PlayAudio(AudioClip[] audio, Vector3 position, float volume)
    {
        PlayAudio(audio[Random.Range(0, audio.Length)], position, volume);
    }
}
