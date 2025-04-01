using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { OVERWORLD, BATTLE, DIALOGUE, CUTSCENE }

public class GameController : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera overworldCamera;

    GameState state;

    private void Awake() {
        ConditionsDataBase.Init();
    }

    private void Start() {
        // battle events
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        // trainer events
        playerController.OnEnterTrainerView += (Collider2D trainerCollider) => {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();

            if (trainer != null) {
                state = GameState.CUTSCENE;
                
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        // dialogue events
        DialogueManager.Instance.OnShowDialogue += () => { state = GameState.DIALOGUE; };
        DialogueManager.Instance.OnCloseDialogue += () => {
            if (state == GameState.DIALOGUE) {
                state = GameState.OVERWORLD;
            } 
        };
    }

    private void StartBattle() {
        state = GameState.BATTLE;

        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    private void EndBattle(bool won) {
        state = GameState.OVERWORLD;

        battleSystem.gameObject.SetActive(false);
        overworldCamera.gameObject.SetActive(true);
    }

    private void Update() {
        if (state == GameState.OVERWORLD) {
            playerController.HandleUpdate();
        } else if (state == GameState.BATTLE) {
            battleSystem.HandleUpdate();
        } else if (state == GameState.DIALOGUE) {
            DialogueManager.Instance.HandleUpdate();
        }
    }
}
