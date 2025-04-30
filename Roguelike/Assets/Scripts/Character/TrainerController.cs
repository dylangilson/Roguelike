using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable {
    [SerializeField] string trainerName;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialogue dialogue;
    [SerializeField] Dialogue dialogueAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    
    Character character;
    bool defeated = false;

    public string TrainerName {
        get {
            return trainerName;
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

    private void Start() {
        SetFOVRotation(character.Animator.DefaultDirection);
    }

    private void Update() {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator) {
        // initiate battle
        if (!defeated) {
            exclamation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            exclamation.SetActive(false);
        }
        
        // face player
        character.LookTowards(initiator.position);

        // show dialogue
        if (!defeated) {
            yield return DialogueManager.Instance.ShowDialogue(dialogue, () => {
                GameController.Instance.StartTrainerBattle(this);
            });
        } else {
            yield return DialogueManager.Instance.ShowDialogue(dialogueAfterBattle);
        }
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
            GameController.Instance.StartTrainerBattle(this);
        }));
    }

    public void DefeatedByPlayer() {
        defeated = true;
        fov.gameObject.SetActive(false);
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
