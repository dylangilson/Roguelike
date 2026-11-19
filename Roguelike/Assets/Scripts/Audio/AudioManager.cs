using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour {
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration = 0.75f;

    float originalMusicVolume;

    public static AudioManager i { get; private set; }

    private void Awake() {
        i = this;
    }

    private void Start() {
        originalMusicVolume = musicPlayer.volume;
    }

    public void PlayMusic(AudioClip clip, bool loop=true, bool fade=false) {
        if (clip == null) {
            return;
        }
        
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade) {
        if (fade) {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;

        musicPlayer.Play();

        if (fade) {
            yield return musicPlayer.DOFade(originalMusicVolume, fadeDuration).WaitForCompletion();
        }
    }
}
