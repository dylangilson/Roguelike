using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { OVERWORLD, BATTLE }

public class GameController : MonoBehaviour {
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera overworldCamera;

    GameState state;

    private void Start() {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    private void StartBattle() {
        state = GameState.BATTLE;

        battleSystem.gameObject.SetActive(true);
        overworldCamera.gameObject.SetActive(false);

        battleSystem.StartBattle();
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
        }
    }
}
