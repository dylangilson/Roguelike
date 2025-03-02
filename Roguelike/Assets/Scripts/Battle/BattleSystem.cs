using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, ACTION_SELECTION, MOVE_SELECTION, RUNNING_TURN, BUSY, PARTY_SCREEN, BATTLE_OVER }
public enum BattleAction { MOVE, SWITCH_POKEMON, USE_ITEM, RUN }

public class BattleSystem : MonoBehaviour {
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;
    
    BattleState state;
    BattleState? previousState;
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

    IEnumerator RunAfterTurn(BattleUnit source) {
        yield return new WaitUntil(() => state == BattleState.RUNNING_TURN);
        
        // pokemon could be burned or poisoned, this deals damage to them after THEIR turn
        source.Pokemon.OnAfterTurn();

        yield return ShowStatusChanges(source.Pokemon);

        if (source == playerUnit) {
            yield return source.HUD.UpdatePlayerHitpoints();
        } else if (source == enemyUnit) {
            yield return source.HUD.UpdateEnemyHitpoints();
        }

        if (source.Pokemon.CurrentHitpoints <= 0) {
            yield return dialogueBox.TypeDialogue($"{source.Pokemon.Blueprint.PokemonName} fainted!");

            source.PlayFaintAnimation();
            
            yield return new WaitForSeconds(2.0f);

            HandleFaint(source);
        }
    }

    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RUNNING_TURN;

        if (playerAction == BattleAction.MOVE) {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            // check who moves first
            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Blueprint.PriorityValue;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Blueprint.PriorityValue;

            bool isPlayerFaster = playerMovePriority > enemyMovePriority;

            if (playerMovePriority == enemyMovePriority) {
                isPlayerFaster = playerUnit.Pokemon.Speed == enemyUnit.Pokemon.Speed;

                if (isPlayerFaster) {
                    int random = UnityEngine.Random.Range(1, 3); // roll a 50/50 to determine which unit goes first

                    if (random == 1) {
                        isPlayerFaster = false;
                    } else if (random == 2) {
                        isPlayerFaster = true;
                    }
                } else if (playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed) {
                    isPlayerFaster = true;
                } else if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed) {
                    isPlayerFaster = false;
                }
            }

            var firstUnit = (isPlayerFaster) ? playerUnit : enemyUnit; 
            var secondUnit = (isPlayerFaster) ? enemyUnit : playerUnit; 

            var secondPokemon = secondUnit.Pokemon;

            yield return PerformMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            } else {
                yield return RunAfterTurn(firstUnit);
            }
            
            if (secondPokemon.CurrentHitpoints > 0){
                yield return PerformMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                
                if (state == BattleState.BATTLE_OVER) {
                    yield break;
                } else {
                    yield return RunAfterTurn(secondUnit);
                }
            }
        } else if (playerAction == BattleAction.SWITCH_POKEMON) { 
            // if player switches out as action for turn
            var selectedMember = playerParty.GetParty()[currentMember];

            state = BattleState.BUSY;

            yield return SwitchPokemon(selectedMember);

            playerUnit.Pokemon = selectedMember;

            yield return RunAfterTurn(playerUnit);

            // enemy Action
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            yield return PerformMove(enemyUnit, playerUnit, enemyUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(enemyUnit);

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            }
        }

        if (state != BattleState.BATTLE_OVER){
            ActionSelection();
        }
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
        if (source.IsPlayerUnit) {
            yield return dialogueBox.TypeDialogue($"{source.Pokemon.Blueprint.PokemonName} used {move.Blueprint.MoveName}!");
        } else {
            yield return dialogueBox.TypeDialogue($"Enemy {source.Pokemon.Blueprint.PokemonName} used {move.Blueprint.MoveName}!");
        }
        if (checkIfMoveHits(move, source.Pokemon, target.Pokemon)) {
            source.PlayAttackAnimation();

            yield return new WaitForSeconds(1.0f);

            target.PlayHitAnimation();

            if (move.Blueprint.MoveCatagory == MoveCatagory.OTHER) {
                yield return RunMoveEffects(move.Blueprint.MoveEffects, source.Pokemon, target.Pokemon, move.Blueprint.MoveTarget);
            } else if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL || move.Blueprint.MoveCatagory == MoveCatagory.SPECIAL) {
                var damageDetails = target.Pokemon.TakeDamage(move, source.Pokemon);
                if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL){
                    Debug.Log($"{target.Pokemon.Blueprint.PokemonName} has {target.Pokemon.Defence} defence");
                } else {
                    Debug.Log($"{target.Pokemon.Blueprint.PokemonName} has {target.Pokemon.SpecialDefence} special defence");
                }

                if (target == playerUnit) {
                    yield return target.HUD.UpdatePlayerHitpoints();
                } else if (target == enemyUnit) {
                    yield return target.HUD.UpdateEnemyHitpoints();
                }
                
                yield return ShowDamageDetails(damageDetails);
            } else {
                Debug.Log($"{move} is not PHYSICAL, SPECIAL, OR OTHER");
            }

            if (move.Blueprint.SecondaryEffects != null && move.Blueprint.SecondaryEffects.Count > 0 && target.Pokemon.CurrentHitpoints > 0) {
                foreach (var secondaryEffect in move.Blueprint.SecondaryEffects) {
                    if (UnityEngine.Random.Range(1, 101) <= secondaryEffect.Chance) {
                        yield return RunMoveEffects(secondaryEffect, source.Pokemon, target.Pokemon, secondaryEffect.Target);
                    }
                }
            }

            if (target.Pokemon.CurrentHitpoints <= 0) {
                yield return dialogueBox.TypeDialogue($"{target.Pokemon.Blueprint.PokemonName} fainted!");

                target.PlayFaintAnimation();
                
                yield return new WaitForSeconds(2.0f);

                HandleFaint(target);
            }
        } else {
            yield return dialogueBox.TypeDialogue($"{source.Pokemon.Blueprint.PokemonName}'s attack missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget) {
        // stat changing move effects
        if (effects.Boosts != null) {
            if (moveTarget == MoveTarget.SELF) {
                source.ApplyBoosts(effects.Boosts);
            } else if (moveTarget == MoveTarget.FOE) {
                target.ApplyBoosts(effects.Boosts);
            } else {
                Debug.Log($"This move has no target");
            }
        }
        
        // status effect move effects
        if (effects.Status != ConditionID.NONE) {
            if (target.Status != null) {
                // this should work but does not, we successfully enter this if statement
                yield return dialogueBox.TypeDialogue($"{source.Blueprint.PokemonName} is already afflicted by {target.Status.Name}!");
            } else {
                target.SetStatus(effects.Status);
            }
        }
    
        // volatile status effect move effects
        if (effects.VolatileStatus != ConditionID.NONE) {
            if (target.VolatileStatus != null) {
                // TODO: does this work?? this should work but does not ; we successfully enter this if statement
                yield return dialogueBox.TypeDialogue($"{source.Blueprint.PokemonName} is already afflicted by {target.VolatileStatus.Name}!");
            } else {
                target.SetVolatileStatus(effects.VolatileStatus);
            }
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    bool checkIfMoveHits(Move move, Pokemon source, Pokemon target) {
        if (move.Blueprint.SkipAccuracyCheck) {
            return true;
        }

        float moveAccuracy = move.Blueprint.Accuracy;
        int accuracy = source.StatBoosts[Stat.ACCURACY];
        int evasion = target.StatBoosts[Stat.EVASION];
        var boostValues = new float[] { 1.0f, 4.0f / 3.0f, 5.0f / 3.0f, 2.0f, 7.0f / 3.0f, 8.0f / 3.0f, 3.0f };

        if (accuracy > 0) {
            moveAccuracy *= boostValues[accuracy];
        } else {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0) {
            moveAccuracy /= boostValues[evasion];
        } else {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy; // roll between 1 and 100
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon) {
        while (pokemon.StatusChanges.Count > 0) {
            var message = pokemon.StatusChanges.Dequeue();

            yield return dialogueBox.TypeDialogue(message);
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
                previousState = state;
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
            var move = playerUnit.Pokemon.Moves[currentMove];

            if (move.PowerPoints == 0) {
                return;
            }

            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.MOVE));

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

            if (previousState == BattleState.ACTION_SELECTION){
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SWITCH_POKEMON));
            } else { // pokemon fainted
                state = BattleState.BUSY;

                StartCoroutine(SwitchPokemon(selectedMember));
            }
            
        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            partyScreen.gameObject.SetActive(false);

            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon) {
        if (playerUnit.Pokemon.CurrentHitpoints > 0) {

            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Blueprint.PokemonName}!");

            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Blueprint.PokemonName}!");

        state = BattleState.RUNNING_TURN;
    }
}
