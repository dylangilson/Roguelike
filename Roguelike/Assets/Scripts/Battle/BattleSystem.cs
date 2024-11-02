using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, PLAYER_ACTION, PLAYER_MOVE, ENEMY_MOVE, BUSY, PARTYSCREEN }

public class BattleSystem : MonoBehaviour {
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;
    
    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    Party playerParty;
    Pokemon wildPokemon;

    public void StartBattle(Party playerParty, Pokemon wildPokemon) {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        playerUnit.Setup(playerParty.GetLeadPokemon());
        playerHUD.SetData(playerUnit.Pokemon);

        enemyUnit.Setup(wildPokemon);
        enemyHUD.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Blueprint.GetPokemonName()} has appeared!");

        PlayerAction();
    }

    void PlayerAction() {
        StartCoroutine(dialogueBox.TypeDialogue("Choose an action!"));

        state = BattleState.PLAYER_ACTION;

        dialogueBox.EnableActionSelector(true);
    }

    void PlayerBag() {
        print("Bag Screen");
    }

    void PlayerPokemon() {
        state =  BattleState.PARTYSCREEN;
        partyScreen.SetPartyData(playerParty.GetParty());
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerRun() {
        print("Player Ran");
    }

    void PlayerMove() {
        state = BattleState.PLAYER_MOVE;

        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove() {
        state = BattleState.BUSY;

        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PowerPoints--;
        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.GetPokemonName()} used {move.Blueprint.GetMoveName()}!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHUD.UpdateEnemyHitpoints();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.GetPokemonName()} fainted!");
            enemyUnit.PlayFaintAnimation();
            
            yield return new WaitForSeconds(2.0f);

            OnBattleOver(true);
        } else {
            StartCoroutine(PerformEnemyMove());
        }
    }

    IEnumerator PerformEnemyMove() {
        state = BattleState.ENEMY_MOVE;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PowerPoints--;
        yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.GetPokemonName()} used {move.Blueprint.GetMoveName()}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHUD.UpdatePlayerHitpoints();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.GetPokemonName()} fainted!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2.0f);

            var nextPokemon = playerParty.GetLeadPokemon();
            if (nextPokemon == null) {
                OnBattleOver(true);
            }
            PlayerPokemon();
        } else {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Critical == 2.0f) {
            yield return dialogueBox.TypeDialogue("A critical hit!");
        }

        if (damageDetails.Effectiveness > 1.0f) {
            yield return dialogueBox.TypeDialogue("It's super effective!");
        } else if (damageDetails.Effectiveness < 1.0f) {
            yield return dialogueBox.TypeDialogue("It's not very effective!");
        }
    }

    public void HandleUpdate() {
        if (state == BattleState.PLAYER_ACTION) {
            HandleActionSelection();
        } else if (state == BattleState.PLAYER_MOVE) {
            HandleMoveSelection();
        } else if (state == BattleState.PARTYSCREEN) {
            HandlePartySelection();
        }
    }

    private void HandleActionSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
                ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                --currentAction;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                currentAction -= 2;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                currentAction += 2;
        }
        currentAction = Mathf.Clamp(currentAction, 0, 3);
    

        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            if (currentAction == 0) {
                PlayerMove();
            } else if (currentAction == 1) {
                // PlayerBag();
            }  else if (currentAction == 2) {
                PlayerPokemon();

            }  else if (currentAction == 3) {
                // PlayerRun();
            }
        }
    }

    private void HandleMoveSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMove;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMove;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMove -= 2;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMove += 2;
        }
        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogueBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(PerformPlayerMove());
        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection(){
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMember;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMember;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMember -= 2;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMember += 2;
        }
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.GetParty().Count - 1);

        partyScreen.UpdateMemberSelect(currentMember);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            var selectedMember = playerParty.GetParty()[currentMember];
            if (selectedMember.CurrentHitpoints <= 0) {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }
            if (selectedMember == playerUnit.Pokemon){
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.BUSY;
            StartCoroutine(SwitchPokemon(selectedMember));

        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon) {
        bool fainted = false;
        if (playerUnit.Pokemon.CurrentHitpoints > 0) {
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Blueprint.GetPokemonName()}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        } else if (playerUnit.Pokemon.CurrentHitpoints <= 0) {
            fainted = true;
        }

        playerUnit.Setup(newPokemon);
        playerHUD.SetData(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Blueprint.GetPokemonName()}!");
        if (fainted){
            PlayerAction();
        } else if (!fainted){
            StartCoroutine(PerformEnemyMove());
        }
    }
}
