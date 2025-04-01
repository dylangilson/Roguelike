using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainerView;

    private Vector2 input;
    private Character character;

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
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
 
        // Debug.DrawLine(transform.position, interactPosition, Color.red, 0.5f);
    }

    private void OnMove() {
        CheckForTrainers();
        CheckForEncounters();
    }
 
    private void CheckForEncounters() {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.GrassLayer) != null) {
            if (UnityEngine.Random.Range(1, 101) <= 10) {
                character.Animator.IsMoving =  false;
                OnEncountered();
            }
        }
    }

    private void CheckForTrainers() {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.FOVLayer);

        if (collider != null) {
            character.Animator.IsMoving =  false;
            OnEnterTrainerView?.Invoke(collider);
        }
    }
}
