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

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Blueprint.PokemonName} has appeared!");

        RollInitiative();
    }

    void RollInitiative() {
        if (playerUnit.Pokemon.Speed == enemyUnit.Pokemon.Speed) {           
            int value = UnityEngine.Random.Range(0, 2); // random number 0 or 1

            if (value == 0) {
                ActionSelection();
            } else if (value == 1) {
                StartCoroutine(PerformEnemyMove());
            } else {
                Debug.Log("Error during speed tie");
            }
        } else if (playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed) {
            ActionSelection();
        } else if (playerUnit.Pokemon.Speed < enemyUnit.Pokemon.Speed) {
            StartCoroutine(PerformEnemyMove());
        } else {
            Debug.Log("Error determining initiaive");
        }
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
        playerParty.GetParty().ForEach(pokemon => pokemon.OnBattleOver());

        OnBattleOver(won);
    }

    IEnumerator PerformMove(BattleUnit source, BattleUnit target, Move move) {
        if (!source.Pokemon.OnBeforeMove()) {
            yield return ShowStatusChanges(source.Pokemon);
            yield return source.HUD.UpdatePlayerHitpoints();
            yield break;
        }

        yield return ShowStatusChanges(source.Pokemon);

        move.PowerPoints--;

        yield return dialogueBox.TypeDialogue($"{source.Pokemon.Blueprint.PokemonName} used {move.Blueprint.MoveName}!");

        source.PlayAttackAnimation();

        yield return new WaitForSeconds(1.0f);

        target.PlayHitAnimation();

        if (move.Blueprint.MoveCatagory == MoveCatagory.OTHER) {
            yield return RunMoveEffects(move, source.Pokemon, target.Pokemon);
        } else if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL || move.Blueprint.MoveCatagory == MoveCatagory.SPECIAL) {
            var damageDetails = target.Pokemon.TakeDamage(move, source.Pokemon);
            
            if (target == playerUnit) {
                yield return target.HUD.UpdatePlayerHitpoints();
            } else if (target == enemyUnit) {
                yield return target.HUD.UpdateEnemyHitpoints();
            }
            
            yield return ShowDamageDetails(damageDetails);
        } else {
            Debug.Log($"{move} is not PHYSICAL, SPECIAL, OR OTHER");
        }

        if (target.Pokemon.CurrentHitpoints <= 0) {
            yield return dialogueBox.TypeDialogue($"{target.Pokemon.Blueprint.PokemonName} fainted!");

            target.PlayFaintAnimation();
            
            yield return new WaitForSeconds(2.0f);

            HandleFaint(target);
        }

        // pokemon could be burned or poisoned, this deals damage to them after THEIR turn
        source.Pokemon.OnAfterTurn();

        yield return ShowStatusChanges(source.Pokemon);
        if (source == playerUnit) {
            yield return source.HUD.UpdatePlayerHitpoints();
        } else if (source == enemyUnit) {
            yield return source.HUD.UpdateEnemyHitpoints();
        }    
        if (source.Pokemon.CurrentHitpoints <= 0) {
            yield return dialogueBox.TypeDialogue($"{target.Pokemon.Blueprint.PokemonName} fainted!");

            target.PlayFaintAnimation();
            
            yield return new WaitForSeconds(2.0f);

            HandleFaint(target);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target) {
        MoveEffects effects = move.Blueprint.MoveEffects;

        // Stat changing move effects
        if (effects.Boosts != null) {
            if (move.Blueprint.MoveTarget == MoveTarget.SELF) {
                source.ApplyBoosts(effects.Boosts);
            } else if (move.Blueprint.MoveTarget == MoveTarget.FOE) {
                target.ApplyBoosts(effects.Boosts);
            } else {
                Debug.Log($"{move} has no target");
            }
        }
        
        // Status effect move effects
        if (effects.Status != ConditionID.NONE) {
            target.SetStatus(effects.Status);
        }

        // Volatile status effect move effects
        if (effects.VolatileStatus != ConditionID.NONE) {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon) {
        while (pokemon.StatusChanges.Count > 0) {
            var message = pokemon.StatusChanges.Dequeue();

            yield return dialogueBox.TypeDialogue(message);
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

    void HandlePartySelection() {
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
        bool fainted = true;

        if (playerUnit.Pokemon.CurrentHitpoints > 0) {
            fainted = false;

            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Blueprint.PokemonName}!");

            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Blueprint.PokemonName}!");

        if (fainted) {
            RollInitiative();
        } else {
            StartCoroutine(PerformEnemyMove());
        }
    }
}
