using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable {
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

    public IEnumerator Interact(Transform initiator) {
        if (state == NPCState.IDLE) {
            state = NPCState.DIALOGUE;
            character.LookTowards(initiator.position);

            yield return DialogueManager.Instance.ShowDialogue(dialogue, () => {
                idleTimer = 0.0f;
                state = NPCState.IDLE;
            });

            // StartCoroutine(character.Move(new Vector2(-3, 0)));
        }
    }

    IEnumerator Walk() {
        state = NPCState.WALKING;

        var oldPosition = transform.position;

        yield return character.Move(movements[index]);

        if (transform.position != oldPosition) {
            index = (index + 1) % movements.Count;
        }

        state = NPCState.IDLE;
    }
    public object CaptureState() {
        float[] position = new float[] { transform.position.x, transform.position.y };
        return position;
    }

    public void RestoreState(object state) {
        var position = (float[])state;
        transform.position = new Vector3(position[0], position[1]);
    }
}

public enum NPCState { IDLE, WALKING, DIALOGUE }
