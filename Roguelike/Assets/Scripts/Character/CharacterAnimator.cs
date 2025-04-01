using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection { UP, DOWN, LEFT, RIGHT }

public class CharacterAnimator : MonoBehaviour {
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.DOWN;

    // parameters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public FacingDirection DefaultDirection {
        get {
            return defaultDirection;
        }
    }

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

        SetFacingDirection(defaultDirection);

        currentAnimation = walkDownAnimation;
    }

    private void Update() {
        var previousAnimation = currentAnimation;
        bool previousMoving = IsMoving;

        if(MoveX == 1.0f) {
            currentAnimation = walkRightAnimation;
        } else if (MoveX == -1.0f) {
            currentAnimation = walkLeftAnimation;
        } else if (MoveY == 1.0f) {
            currentAnimation = walkUpAnimation;
        } else if (MoveY == -1.0f) {
            currentAnimation = walkDownAnimation;
        }

        if (currentAnimation != previousAnimation || IsMoving != previousMoving) {
            currentAnimation.Start();
        }

        if (IsMoving) {
            currentAnimation.HandleUpdate();
        } else {
            spriteRenderer.sprite = currentAnimation.Frames[0];
        }

        previousMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection direction) {
        if (direction == FacingDirection.RIGHT) {
            MoveX = 1.0f;
        } else if (direction == FacingDirection.LEFT) {
            MoveX = -1.0f;
        } else if (direction == FacingDirection.UP) {
            MoveY = 1.0f;
        } else if (direction == FacingDirection.DOWN) {
            MoveY = -1.0f;
        } else {
            Debug.LogError("Invalid Move Direction!");
        }
    }
}
