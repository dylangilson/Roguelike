using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public float movementSpeed;

    public bool IsMoving { get; private set; }

    CharacterAnimator animator;

    public CharacterAnimator Animator {
        get {
            return animator;
        }
    }

    private void Awake() {
        animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 movementVector, Action OnMoveOver=null) {
        animator.MoveX = Mathf.Clamp(movementVector.x, -1.0f, 1.0f);
        animator.MoveY = Mathf.Clamp(movementVector.y, -1.0f, 1.0f);

        var targetPosition = transform.position;
        targetPosition.x += movementVector.x;
        targetPosition.y += movementVector.y;

        if (!IsWalkable(targetPosition)) {
            yield break;
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

    private bool IsWalkable(Vector3 targetPosition) {
        return Physics2D.OverlapCircle(targetPosition, 0.3f, GameLayers.Instance.SolidObjectsLayer | GameLayers.Instance.InteractableLayer) == null;
    }
}
