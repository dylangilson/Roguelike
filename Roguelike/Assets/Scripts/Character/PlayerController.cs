using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] string playerName;
    [SerializeField] Sprite sprite;

    private Vector2 input;
    private Character character;

    public string PlayerName {
        get {
            return playerName;
        }
    }

    public Sprite Sprite {
        get {
            return sprite;
        }
    }

    private void Awake() {
        character = GetComponent<Character>();
    }

    public void HandleUpdate() {
        if (!character.IsMoving) {
            // get input
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if (input.x != 0) {
                input.y = 0;
            }

            // move
            if (input != Vector2.zero) {
                StartCoroutine(character.Move(input, OnMove));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            Interact();
        }
    }

    void Interact() {
        var facingDirection = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPosition = transform.position + facingDirection;

        var collider = Physics2D.OverlapCircle(interactPosition, 0.3f, GameLayers.Instance.InteractableLayer);
        
        if (collider != null) {
            var interactable = collider.GetComponent<Interactable>();

            if (interactable != null) {
                StartCoroutine(interactable.Interact(transform));
            }
        }
    }

    private void OnMove() {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 0.2f, GameLayers.Instance.TriggerableLayers);

        foreach (var collider in colliders) {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();

            if (triggerable != null) {
                character.Animator.IsMoving = false;
                triggerable.OnPlayerTriggered(this);

                break;
            }
        }
    }
}
