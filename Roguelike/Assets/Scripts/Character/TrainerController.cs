using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour {
    [SerializeField] Dialogue dialogue;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    
    Character character;

    private void Awake() {
        character = GetComponent<Character>();
    }

    private void Start() {
        SetFOVRotation(character.Animator.DefaultDirection);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player) {
        // initiate battle
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // move to player
        var difference = player.transform.position - transform.position;
        var movementVector = difference - difference.normalized;
        movementVector = new Vector2(Mathf.Round(movementVector.x), Mathf.Round(movementVector.y));

        yield return character.Move(movementVector);

        // show dialogue
        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue, () => {
            Debug.Log("Battle started");
        }));
    }

    public void SetFOVRotation(FacingDirection direction) {
        float angle = 0.0f;

        if (direction == FacingDirection.RIGHT) {
            angle = 90.0f;
        } else if (direction == FacingDirection.LEFT) {
            angle = 270.0f;
        } else if (direction == FacingDirection.UP) {
            angle = 180.0f;
        } else if (direction == FacingDirection.DOWN) {
            angle = 0.0f;
        } else {
            Debug.LogError("Invalid Facing Direction!");
        }

        fov.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);
    }
}
