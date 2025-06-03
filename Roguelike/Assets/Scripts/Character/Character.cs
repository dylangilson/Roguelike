using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public float movementSpeed;

    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    public CharacterAnimator Animator {
        get {
            return animator;
        }
    }

    private void Awake() {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public void SetPositionAndSnapToTile(Vector2 position) {
        position.x = Mathf.Floor(position.x) + 0.5f;
        position.y = Mathf.Floor(position.y) + 0.5f + OffsetY;

        transform.position = position;
    }

    public IEnumerator Move(Vector2 movementVector, Action OnMoveOver = null) {
        animator.MoveX = Mathf.Clamp(movementVector.x, -1.0f, 1.0f);
        animator.MoveY = Mathf.Clamp(movementVector.y, -1.0f, 1.0f);
        
        var targetPosition = transform.position;
        targetPosition.x += movementVector.x;
        targetPosition.y += movementVector.y;

        if (!IsPathClear(targetPosition)) {
            var nextPosition = transform.position;
            var failSafeCounter = 10;
            
            while (!IsPathClear(nextPosition) && failSafeCounter > 0) {
                failSafeCounter--;

                if (movementVector.x > 0) {
                    nextPosition.x += 1;
                } else if (movementVector.x < 0) {
                    nextPosition.x -= 1;
                } else if (movementVector.y > 0) {
                    nextPosition.y += 1;
                } else if (movementVector.y < 0) {
                    nextPosition.y -= 1;
                } 
            }

            if (failSafeCounter == 0) {
                yield break;
            }

            targetPosition = nextPosition;
        }

        IsMoving = true;

        while ((targetPosition - transform.position).sqrMagnitude > Mathf.Epsilon) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
            
            yield return null;
        }

        transform.position = targetPosition;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate() {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPosition) {
        var difference = targetPosition - transform.position;
        var direction = difference.normalized;
        
        if (Physics2D.BoxCast(transform.position + direction, new Vector2(0.2f, 0.2f), 0.0f, 
              direction, difference.magnitude - 1,  GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer) == true) {
                return false; // path is blocked
        };
            
        return true;
    }

    public void LookTowards(Vector3 targetPosition) {
        var xdifference = Mathf.Floor(targetPosition.x) - Mathf.Floor(transform.position.x);
        var ydifference = Mathf.Floor(targetPosition.y) - Mathf.Floor(transform.position.y);

        if (xdifference == 0 || ydifference == 0) {
            animator.MoveX = Mathf.Clamp(xdifference, -1.0f, 1.0f);
            animator.MoveY = Mathf.Clamp(ydifference, -1.0f, 1.0f);

        } else{
            Debug.LogError("Error in LookTowards() in Character.cs: targetPosition is diagonal to NPC");
        }
    }

    private bool IsWalkable(Vector3 targetPosition) {
        return Physics2D.OverlapCircle(targetPosition, 0.3f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer) == null;
    }
}
