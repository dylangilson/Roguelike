using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour {
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHUD hud;

    public Pokemon Pokemon { get; set; }
    public bool IsPlayerUnit {
        get {
            return isPlayerUnit;
        }
    }
    public BattleHUD HUD {
        get {
            return hud;
        }
    }

    Image image;
    Vector3 originalPosition;
    Color originalColour;

    private void Awake() {
        image = GetComponent<Image>();
        
        originalPosition = image.transform.localPosition;
        originalColour = image.color;
    }

    public void Setup(Pokemon pokemon) {
        Pokemon = pokemon;

        if (isPlayerUnit) {
            image.sprite = Pokemon.Blueprint.BackSprite;
        } else {
            image.sprite = Pokemon.Blueprint.FrontSprite;
        }

        hud.gameObject.SetActive(true);
        hud.SetData(pokemon);

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        image.color = originalColour;

        PlayEnterAnimation();
    }

    public void Clear() {
        hud.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation() {
        if (isPlayerUnit) {
            image.transform.localPosition = new Vector3(-500.0f, originalPosition.y);
        } else {
            image.transform.localPosition = new Vector3(500.0f, originalPosition.y);
        }

        image.transform.DOLocalMoveX(originalPosition.x, 1.0f);
    }

    public void PlayAttackAnimation() {
        var sequence = DOTween.Sequence();

        if (isPlayerUnit) {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50.0f, 0.25f));
        } else {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50.0f, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));
    }

    public void PlayHitAnimation() {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColour, 0.1f));
    }

    public void PlayFaintAnimation() {
        var sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 150.0f, 0.5f));
        sequence.Join(image.DOFade(0.0f, 0.5f));
    }

    public IEnumerator PlayCatchAnimation() {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOFade(0.0f, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPosition.y + 50.0f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1.0f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation() {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOFade(1.0f, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPosition.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
