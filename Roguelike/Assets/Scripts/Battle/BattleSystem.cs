using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, ACTION_SELECTION, MOVE_SELECTION, PERFORM_MOVE, BUSY, PARTY_SCREEN, BATTLE_OVER }

public class BattleSystem : MonoBehaviour {
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
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
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Blueprint.GetPokemonName()} has appeared!");

        ActionSelection();
    }

    void ActionSelection() {
        StartCoroutine(dialogueBox.TypeDialogue("Choose an action!"));

        state = BattleState.ACTION_SELECTION;

        dialogueBox.EnableActionSelector(true);
    }

    void PlayerBag() {
        print("Bag Screen");
    }

    void PlayerPokemon() {
        state = BattleState.PARTY_SCREEN;
        partyScreen.SetPartyData(playerParty.GetParty());
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerRun() {
        print("Player Ran");
    }

    void MoveSelection() {
        state = BattleState.MOVE_SELECTION;

        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    void HandleFaint(BattleUnit pokemon) {
        if (pokemon.IsPlayerUnit) {
            var nextPokemon = playerParty.GetLeadPokemon();

            if (nextPokemon != null) {
                PlayerPokemon();
            } else {
                BattleOver(false);
            }
        } else {
            BattleOver(true);
        }
    }

    void BattleOver(bool won) {
        state = BattleState.BATTLE_OVER;

        OnBattleOver(won);
    }

    IEnumerator PerformMove(BattleUnit source, BattleUnit target, Move move) {
        move.PowerPoints--;

        yield return dialogueBox.TypeDialogue($"{source.Pokemon.Blueprint.GetPokemonName()} used {move.Blueprint.GetMoveName()}!");

        source.PlayAttackAnimation();

        yield return new WaitForSeconds(1.0f);

        target.PlayHitAnimation();

        var damageDetails = target.Pokemon.TakeDamage(move, source.Pokemon);

        yield return target.HUD.UpdateEnemyHitpoints();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {
            yield return dialogueBox.TypeDialogue($"{target.Pokemon.Blueprint.GetPokemonName()} fainted!");

            target.PlayFaintAnimation();
            
            yield return new WaitForSeconds(2.0f);

            HandleFaint(target);
        }
    }

    IEnumerator PerformPlayerMove() {
        state = BattleState.PERFORM_MOVE;

        var move = playerUnit.Pokemon.Moves[currentMove];
        
        yield return PerformMove(playerUnit, enemyUnit, move);

        // if PerformMove() resulted in a pokemon fainting, then the state will not be BattleState.PERFORM_MOVE
        if (state == BattleState.PERFORM_MOVE) {
            StartCoroutine(PerformEnemyMove());
        }
    }

    IEnumerator PerformEnemyMove() {
        state = BattleState.PERFORM_MOVE;

        var move = enemyUnit.Pokemon.GetRandomMove();

        yield return PerformMove(enemyUnit, playerUnit, move);

        // if PerformMove() resulted in a pokemon fainting, then the state will not be BattleState.PERFORM_MOVE
        if (state == BattleState.PERFORM_MOVE) {
            ActionSelection();
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
        if (state == BattleState.ACTION_SELECTION) {
            HandleActionSelection();
        } else if (state == BattleState.MOVE_SELECTION) {
            HandleMoveSelection();
        } else if (state == BattleState.PARTY_SCREEN) {
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
                MoveSelection();
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

            ActionSelection();
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
                partyScreen.SetMessageText("You can't send out a fainted pokemon!");
                return;
            }
            if (selectedMember == playerUnit.Pokemon){
                partyScreen.SetMessageText("You can't switch with the same pokemon!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            state = BattleState.BUSY;

            StartCoroutine(SwitchPokemon(selectedMember));

        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            partyScreen.gameObject.SetActive(false);

            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon) {
        bool fainted = false;
        if (playerUnit.Pokemon.CurrentHitpoints > 0) {
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Blueprint.GetPokemonName()}!");

            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        } else if (playerUnit.Pokemon.CurrentHitpoints <= 0) {
            fainted = true;
        }

        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Blueprint.GetPokemonName()}!");

        if (fainted) {
            ActionSelection();
        } else if (!fainted) {
            StartCoroutine(PerformEnemyMove());
        }
    }
}
