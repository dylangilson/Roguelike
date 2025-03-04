using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator {
    List<Sprite> frames;
    SpriteRenderer spriteRenderer;
    float frameRate;

    int currentframe;
    float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate=0.16f) {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start() {
        currentframe = 1;
        timer = 0.0f;
        spriteRenderer.sprite = frames[1];
    }

    public void HandleUpdate() {
        timer += Time.deltaTime;
        if (timer > frameRate) {
            currentframe = (currentframe + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentframe];
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames {
        get { return frames;}
    }

}
