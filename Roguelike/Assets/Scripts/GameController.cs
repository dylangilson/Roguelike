using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { OVERWORLD, BATTLE, DIALOGUE, MENU, CUTSCENE, PAUSED }

public class GameController : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera overworldCamera;

    GameState state;
    GameState stateBeforePause;
    TrainerController trainer;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake() {
        Instance = this;

        menuController = GetComponent<MenuController>();

        PokemonDataBase.Init();
        MoveDataBase.Init();
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
        
        // menu events
        menuController.onMenuItemSelected += OnMenuItemSelected;
        menuController.onBack += () => { state = GameState.OVERWORLD; };
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

            if (Input.GetKeyDown(KeyCode.Escape)) {
                state = GameState.MENU;

                menuController.OpenMenu();
            }

            if (Input.GetKeyDown(KeyCode.C)) {
                Time.timeScale += 0.5f;
                Debug.Log($"Current game speed is {Time.timeScale}");
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                Time.timeScale -= 0.5f;
                Debug.Log($"Current game speed is {Time.timeScale}");
            }
        } else if (state == GameState.BATTLE) {
            battleSystem.HandleUpdate();
        } else if (state == GameState.DIALOGUE) {
            DialogueManager.Instance.HandleUpdate();
        } else if (state == GameState.MENU) {
            menuController.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene) {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuItemSelected(int selectedItem) {
        if (selectedItem == 0) {
            // Pokedex
        } else if (selectedItem == 1) {
            // Pokemon
        } else if (selectedItem == 2) {
            // Bag
        } else if (selectedItem == 3) {
            // User
        } else if (selectedItem == 4) {
            // Save
            SavingSystem.i.Save("saveSlot1");

            state = GameState.OVERWORLD;
        } else if (selectedItem == 5) {
            // Load
            SavingSystem.i.Load("saveSlot1");

            state = GameState.OVERWORLD;
        } else if (selectedItem == 6) {
            // Options
        } else if (selectedItem == 7) {
            // Exit
            state = GameState.OVERWORLD;
        }
    }
}
