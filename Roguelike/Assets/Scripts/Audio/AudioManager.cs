using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class AudioManager : MonoBehaviour {
    
    [SerializeField] List<AudioData> sfxList;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration = 0.75f;

    float originalMusicVolume;
    Dictionary<AudioID, AudioData> sfxLookup;

    public static AudioManager i { get; private set; }

    private void Awake() {
        i = this;
    }

    private void Start() {
        originalMusicVolume = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }
    
    public void PlaySfx(AudioClip clip) {
        if (clip == null) {
            return;
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioID audioID) {
        if (!sfxLookup.ContainsKey(audioID)) {
            return; 
        }
        var audioData = sfxLookup[audioID];
        PlaySfx(audioData.clip); 
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

public enum AudioID { UI_SELECT, HIT, FAINT, EXPGAIN }

[System.Serializable]
public class AudioData {
    public AudioID id; 
    public AudioClip clip;
}