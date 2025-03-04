using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour {
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;

    // parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }

    // states
    SpriteAnimator walkDownAnimation;
    SpriteAnimator walkUpAnimation;
    SpriteAnimator walkRightAnimation;
    SpriteAnimator walkLeftAnimation;

    SpriteAnimator currentAnimation;

    SpriteRenderer spriteRenderer;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkDownAnimation = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnimation = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnimation = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnimation = new SpriteAnimator(walkLeftSprites, spriteRenderer);

        currentAnimation = walkDownAnimation;
    }

    private void Update() {
        var previousAnimation = currentAnimation;
        bool perviousMoving = IsMoving;

        if(MoveX == 1) {
            currentAnimation = walkRightAnimation;
        } else if (MoveX == -1) {
            currentAnimation = walkLeftAnimation;
        } else if (MoveY == 1) {
            currentAnimation = walkUpAnimation;
        } else if (MoveY == -1) {
            currentAnimation = walkDownAnimation;
        }

        if (currentAnimation != previousAnimation || IsMoving != perviousMoving) {
            currentAnimation.Start();
        }

        if (IsMoving) {
            currentAnimation.HandleUpdate();
        } else {
            spriteRenderer.sprite = currentAnimation.Frames[0];
        }
        perviousMoving = IsMoving;
    }
}