using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { OVERWORLD, BATTLE, DIALOGUE, CUTSCENE, PAUSED }

public class GameController : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera overworldCamera;

    GameState state;
    GameState stateBeforePause;
    TrainerController trainer;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    public static GameController Instance { get; private set; }

    private void Awake() {
        Instance = this;
        ConditionsDataBase.Init();
    }

    private void Start() {
        // battle events
        battleSystem.OnBattleOver += EndBattle;

        // dialogue events
        DialogueManager.Instance.OnShowDialogue += () => { state = GameState.DIALOGUE; };
        DialogueManager.Instance.OnCloseDialogue += () => {
            if (state == GameState.DIALOGUE) {
                state = GameState.OVERWORLD;
            } 
        };
    }

    public void PauseGame(bool pause) {
        if (pause) {
            stateBeforePause = state;
            state = GameState.PAUSED;
        } else {
            state = stateBeforePause;
        }
    }

    public void StartBattle() {
        state = GameState.BATTLE;

        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Blueprint, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    public void StartTrainerBattle(TrainerController trainer) {
        state = GameState.BATTLE;

        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<Party>();
        var trainerParty = trainer.GetComponent<Party>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer) {
        state = GameState.CUTSCENE;
                
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    private void EndBattle(bool won) {
        if (trainer != null && won) {
            trainer.DefeatedByPlayer();
            trainer = null;
        }

        state = GameState.OVERWORLD;

        battleSystem.gameObject.SetActive(false);
        overworldCamera.gameObject.SetActive(true);
    }

    private void Update() {
        if (state == GameState.OVERWORLD) {
            playerController.HandleUpdate();
            if (Input.GetKeyDown(KeyCode.S)) {
                SavingSystem.i.Save("saveSlot1");
            }
            
            if (Input.GetKeyDown(KeyCode.L)) {
                SavingSystem.i.Load("saveSlot1");
            }
        } else if (state == GameState.BATTLE) {
            battleSystem.HandleUpdate();
        } else if (state == GameState.DIALOGUE) {
            DialogueManager.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene) {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }
}
