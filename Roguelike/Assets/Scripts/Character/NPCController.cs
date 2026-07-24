using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable {
    [SerializeField] Dialogue dialogue;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector2> movements;
    [SerializeField] float timeBetweenMovements;

    NPCState state;
    float idleTimer = 0.0f;
    int index = 0;
    Quest activeQuest;
    Character character;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;

    private void Awake() {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
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

            if (questToComplete != null) {
                var quest = new Quest(questToComplete);

                yield return quest.CompleteQuest(initiator);

                Debug.Log($"{questToComplete.QuestName} completed!");

                questToComplete = null;
            }

            if (itemGiver != null && itemGiver.CanBeGiven()) {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            } else if (pokemonGiver != null && pokemonGiver.CanBeGiven()) {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            } else if (questToStart != null) {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted()) {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            } else if (activeQuest != null) {
                if (activeQuest.CanBeCompleted()) {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                } else {
                    yield return DialogueManager.Instance.ShowDialogue(activeQuest.Base.InProgressDialogue);
                }
            } else {
                yield return DialogueManager.Instance.ShowDialogue(dialogue);
            }

            idleTimer = 0.0f;
            state = NPCState.IDLE;
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
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null) {
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();
        }
        
        if (questToComplete != null) {
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();
        }

        return saveData;
    }

    public void RestoreState(object state) {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null) {
            activeQuest = (saveData.activeQuest != null)? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null)? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null)? new Quest(saveData.questToComplete).Base : null;
        
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData {
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { IDLE, WALKING, DIALOGUE }
