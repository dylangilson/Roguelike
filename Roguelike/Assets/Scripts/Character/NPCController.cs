using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable {
    [SerializeField] Dialogue dialogue;
    [SerializeField] List<Vector2> movements;
    [SerializeField] float timeBetweenMovements;

    NPCState state;
    float idleTimer = 0.0f;
    int index = 0;
    Character character;

    private void Awake() {
        character = GetComponent<Character>();
    }

    private void Update() {
        if (DialogueManager.Instance.IsShowing) {
            return;
        }

        if (state == NPCState.IDLE) {
            idleTimer += Time.deltaTime;

            if (idleTimer > timeBetweenMovements) {
                idleTimer = 0.0f;

                if (movements.Count > 0) {
                    StartCoroutine(Walk());
                }
            }
        }

        character.HandleUpdate();
    }

    public void Interact() {
        if (state == NPCState.IDLE) {
            StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue));
            // StartCoroutine(character.Move(new Vector2(-3, 0)));
        }
    }

    IEnumerator Walk() {
        state = NPCState.WALKING;

        yield return character.Move(movements[index++]);

        index = index % movements.Count;

        state = NPCState.IDLE;
    }
}

public enum NPCState { IDLE, WALKING }
