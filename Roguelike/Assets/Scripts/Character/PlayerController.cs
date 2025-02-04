using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float movementSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask grassLayer;
    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate() {
        if (!isMoving) {
            // get input
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if (input.x != 0) {
                input.y = 0;
            }

            // move
            if (input != Vector2.zero) {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPosition = transform.position;
                targetPosition.x += input.x;
                targetPosition.y += input.y;

                if (IsWalkable(targetPosition)) {
                    StartCoroutine(Move(targetPosition));
                }
            }
        }
        
        animator.SetBool("isMoving", isMoving);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            Interact();
        }
    }

    void Interact() {
        var facingDirection = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPosition = transform.position + facingDirection;

        var collider = Physics2D.OverlapCircle(interactPosition, 0.3f, interactableLayer);
        if (collider != null) {
            collider.GetComponent<Interactable>()?.Interact();
        }
        // Debug.DrawLine(transform.position, interactPosition, Color.red, 0.5f);
    }

    IEnumerator Move(Vector3 targetPosition) {
        isMoving = true;

        while ((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            
            yield return null;
        }

        transform.position = targetPosition;

        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPosition) {
        return Physics2D.OverlapCircle(targetPosition, 0.3f, solidObjectsLayer | interactableLayer) == null;
    }
 
    private void CheckForEncounters() {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null) {
            if (UnityEngine.Random.Range(1, 101) <= 10) {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }
}
