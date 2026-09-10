using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fader : MonoBehaviour {
    public static Fader i { get; private set; }

    Image image;

    private void Awake() {
        i = this;
        image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float length) {
        yield return image.DOFade(1.0f, length).WaitForCompletion();
    }

    public IEnumerator FadeOut(float length) {
        yield return image.DOFade(0.0f, length).WaitForCompletion();
    }
}
