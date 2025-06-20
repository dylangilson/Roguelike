using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fader : MonoBehaviour {
    Image image;

    private void Awake() {
        image = GetComponent<Image>();
    }

    public IEnumerator FadeIn(float length) {
        yield return image.DOFade(1.0f, length).WaitForCompletion();
    }

    public IEnumerator FadeOut(float length) {
        yield return image.DOFade(0.0f, length).WaitForCompletion();
    }
}
