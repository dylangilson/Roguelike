using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { ACTION_SELECTION, BAG, BATTLE_OVER, BUSY, MOVE_SELECTION, MOVE_TO_FORGET, PARTY_SCREEN, RUNNING_TURN, START, SWITCHING }
public enum BattleAction { MOVE, SWITCH_POKEMON, USE_ITEM, RUN }

public class BattleSystem : MonoBehaviour {
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;
    
    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;
    bool switchingChoice = true;
    Party playerParty;
    Party trainerParty;
    Pokemon wildPokemon;
    bool isTrainerbattle = false;
    PlayerController player;
    TrainerController trainer;
    int runAttempts;
    MoveBase moveToLearn;

    public void StartBattle(Party playerParty, Pokemon wildPokemon) {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        isTrainerbattle = false;
        player = playerParty.GetComponent<PlayerController>();

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(Party playerParty, Party trainerParty) {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerbattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerbattle) {
            // wild pokemon
            playerUnit.Setup(playerParty.GetLeadPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
            
            yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Blueprint.PokemonName} has appeared!");
        } else {
            // trainer battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogueBox.TypeDialogue($"{trainer.TrainerName} wants to battle!");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetLeadPokemon();
            enemyUnit.Setup(enemyPokemon);

            yield return dialogueBox.TypeDialogue($"{trainer.TrainerName} sends out {enemyPokemon.Blueprint.PokemonName}!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetLeadPokemon();
            playerUnit.Setup(playerPokemon);

            yield return dialogueBox.TypeDialogue($"Go {playerPokemon.Blueprint.PokemonName}!");
            dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        runAttempts = 0;

        partyScreen.Init();
        ActionSelection();
    }

    void ActionSelection() {
        StartCoroutine(dialogueBox.TypeDialogue("Choose an action!"));

        state = BattleState.ACTION_SELECTION;

        dialogueBox.EnableActionSelector(true);
    }

    void PlayerBag() {
        state = BattleState.BAG;
        inventoryUI.gameObject.SetActive(true);

    }

    void PlayerPokemon() {
        partyScreen.CalledFrom = state;
        state = BattleState.PARTY_SCREEN;
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection() {
        state = BattleState.MOVE_SELECTION;

        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator TrainerPokemonFainted(Pokemon newPokemon) {
        state = BattleState.BUSY;

        yield return dialogueBox.TypeDialogue($"{trainer.TrainerName} is about to use {newPokemon.Blueprint.PokemonName}. Do you want to switch pokemon?");

        state = BattleState.SWITCHING;

        dialogueBox.EnableChoiceBox(true);
    }

    IEnumerator RunAfterTurn(BattleUnit source) {
        yield return new WaitUntil(() => state == BattleState.RUNNING_TURN);
        
        // pokemon could be burned or poisoned, this deals damage to them after THEIR turn
        source.Pokemon = source.Pokemon.OnAfterTurn();
        Debug.Log($"{source.Pokemon.Blueprint.PokemonName} has {source.Pokemon.CurrentHitpoints} hitpoints");

        yield return ShowStatusChanges(source.Pokemon);
        yield return source.HUD.WaitForHitpointsUpdate();

        if (source.Pokemon.CurrentHitpoints <= 0) {
            yield return HandlePokemonFainted(source);

            yield return new WaitUntil(() => state == BattleState.RUNNING_TURN);
        }
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove) {
        state = BattleState.BUSY;

        yield return dialogueBox.TypeDialogue("Choose a move you want to forget!");

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Blueprint).ToList(), newMove);

        moveToLearn = newMove;

        state = BattleState.MOVE_TO_FORGET;
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
            var selectedMember = partyScreen.SelectedMember;

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
        } else if (playerAction == BattleAction.USE_ITEM) {
            dialogueBox.EnableActionSelector(false);

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            }

            // enemy still gets a move if Pokémon was not caught
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            yield return PerformMove(enemyUnit, playerUnit, enemyUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(enemyUnit);

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            }
        } else if (playerAction == BattleAction.RUN) {
            yield return TryToEscape();

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            }

            // enemy still gets a move if run was not successful
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            yield return PerformMove(enemyUnit, playerUnit, enemyUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(enemyUnit);

            if (state == BattleState.BATTLE_OVER) {
                yield break;
            }
        }

        if (state != BattleState.BATTLE_OVER) {
            dialogueBox.EnableActionSelector(true);
            ActionSelection();
        }
    }

    void HandleFaint(BattleUnit pokemon) {
        pokemon.Pokemon.CureStatus();
        pokemon.Pokemon.CureVolatileStatus();

        if (pokemon.IsPlayerUnit) {
            var nextPokemon = playerParty.GetLeadPokemon();

            if (nextPokemon != null) {
                PlayerPokemon();
            } else {
                BattleOver(false);
            }
        } else {
            if (!isTrainerbattle) {
                BattleOver(true);
            } else {
                var nextPokemon = trainerParty.GetLeadPokemon();

                if (nextPokemon != null) {
                    StartCoroutine(TrainerPokemonFainted(nextPokemon));
                } else {
                    BattleOver(true);
                }
            }
        }
    }

    void BattleOver(bool won) {
        currentAction = 0;
        dialogueBox.UpdateActionSelection(currentAction);
        state = BattleState.BATTLE_OVER;
        playerParty.GetParty().ForEach(pokemon => pokemon.OnBattleOver());
        playerUnit.HUD.ClearData();
        enemyUnit.HUD.ClearData();

        OnBattleOver(won);
    }

    IEnumerator PerformMove(BattleUnit source, BattleUnit target, Move move) {
        if (!source.Pokemon.OnBeforeMove()) {
            yield return ShowStatusChanges(source.Pokemon);
            yield return target.HUD.WaitForHitpointsUpdate();
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

            foreach (var secondaryEffect in move.Blueprint.SecondaryEffects) {
                if (secondaryEffect.Target == MoveTarget.FOE) {
                    yield return new WaitForSeconds(1.0f);

                    target.PlayHitAnimation();
                    break;
                }
            }

            if (move.Blueprint.MoveCatagory == MoveCatagory.OTHER) {
                yield return RunMoveEffects(move.Blueprint.MoveEffects, source.Pokemon, target.Pokemon, move.Blueprint.MoveTarget);
            } else if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL || move.Blueprint.MoveCatagory == MoveCatagory.SPECIAL) {
                DamageDetails damageDetails;
                if (move.Blueprint.MoveTarget == MoveTarget.FOE) {
                    damageDetails = target.Pokemon.TakeDamage(move, source.Pokemon);
                } else {
                    damageDetails = source.Pokemon.TakeDamage(move, source.Pokemon);
                }

                if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL){
                    Debug.Log($"{target.Pokemon.Blueprint.PokemonName} has {target.Pokemon.Defence} defence");
                } else {
                    Debug.Log($"{target.Pokemon.Blueprint.PokemonName} has {target.Pokemon.SpecialDefence} special defence");
                }
                
                yield return target.HUD.WaitForHitpointsUpdate();
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
                yield return HandlePokemonFainted(target);
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
            if (moveTarget == MoveTarget.SELF) {
                if (source.Status != null) {
                    yield return dialogueBox.TypeDialogue($"{source.Blueprint.PokemonName} is already afflicted by {source.Status.Name}!");
                } else {
                    source.SetStatus(effects.Status);
                }
            } else if (moveTarget == MoveTarget.FOE) {
                if (target.Status != null) {
                    yield return dialogueBox.TypeDialogue($"{target.Blueprint.PokemonName} is already afflicted by {target.Status.Name}!");
                } else {
                    target.SetStatus(effects.Status);
                }
            }
        }
    
        // volatile status effect move effects
        if (effects.VolatileStatus != ConditionID.NONE) {
            if (moveTarget == MoveTarget.FOE) {
                if (target.VolatileStatus != null) {
                    yield return dialogueBox.TypeDialogue($"{target.Blueprint.PokemonName} is already afflicted by {target.VolatileStatus.Name}!");
                } else {
                    target.SetVolatileStatus(effects.VolatileStatus);
                }
            } else if (moveTarget == MoveTarget.SELF) {
                if (source.VolatileStatus != null) {
                    yield return dialogueBox.TypeDialogue($"{source.Blueprint.PokemonName} is already afflicted by {source.VolatileStatus.Name}!");
                } else {
                    source.SetVolatileStatus(effects.VolatileStatus);
                }
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
        int evasion;
        if (move.Blueprint.MoveTarget == MoveTarget.FOE){
            evasion = target.StatBoosts[Stat.EVASION];
        } else {
            evasion = 0;
        }
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

    IEnumerator HandlePokemonFainted(BattleUnit targetUnit) {
        yield return dialogueBox.TypeDialogue($"{targetUnit.Pokemon.Blueprint.PokemonName} fainted!");

        targetUnit.PlayFaintAnimation();
        
        yield return new WaitForSeconds(2.0f);

        if (!targetUnit.IsPlayerUnit) {
            int expYield = targetUnit.Pokemon.Blueprint.ExpYield;
            int enemyLevel = targetUnit.Pokemon.Level;
            float trainerBonus = (isTrainerbattle) ? 1.5f : 1.0f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7.0f);

            playerUnit.Pokemon.Exp += expGain;

            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} gained {expGain} experience!");

            yield return playerUnit.HUD.SetExpSmooth(false);

            while (playerUnit.Pokemon.CheckForLevelUp()) {
                yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} is now level {playerUnit.Pokemon.Level}!");
                playerUnit.HUD.SetLevel();

                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newMove != null && !playerUnit.Pokemon.GetMoveNames().Contains(newMove.GetBase().MoveName)) {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MAX_NUMBER_OF_MOVES) {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} learned {newMove.GetBase().MoveName}!");
                        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);
                    } else {
                        yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} is trying to learn {newMove.GetBase().MoveName}!");
                        yield return dialogueBox.TypeDialogue($"But it cannot learn more than {PokemonBase.MAX_NUMBER_OF_MOVES} moves!");

                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.GetBase());

                        yield return new WaitUntil(() => state != BattleState.MOVE_TO_FORGET);
                        yield return new WaitForSeconds(2.0f);
                        
                    }
                }
                yield return playerUnit.HUD.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1.0f);
        }

        HandleFaint(targetUnit);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Critical == 2.0f) {
            yield return dialogueBox.TypeDialogue("A critical hit!");
        }

        if (damageDetails.Effectiveness > 2.0f) {
            yield return dialogueBox.TypeDialogue("It's extremely effective!");
        } else if (damageDetails.Effectiveness > 1.0f) {
            yield return dialogueBox.TypeDialogue("It's super effective!");
        } else if (damageDetails.Effectiveness < 0.5f) {
            yield return dialogueBox.TypeDialogue("It's extremely ineffective!");
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
        } else if (state == BattleState.BAG) {
            Action onBack = () => {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ACTION_SELECTION;
            };

            Action onItemUsed = () => {
                state = BattleState.BUSY;
                inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.USE_ITEM));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        } else if (state == BattleState.SWITCHING) {
            HandleSwitching();
        } else if (state == BattleState.MOVE_TO_FORGET) {
            Action<int> onMoveSelected = (moveIndex) => {
                moveSelectionUI.gameObject.SetActive(false);

                if (moveIndex == PokemonBase.MAX_NUMBER_OF_MOVES) {
                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} did not learn {moveToLearn.MoveName}!"));
                } else {
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Blueprint;

                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Blueprint.PokemonName} forgot {selectedMove.MoveName} and learned {moveToLearn.MoveName}"));
                    // yield return new WaitForSeconds(1.0f);

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RUNNING_TURN;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
        
        if (Input.GetKeyDown(KeyCode.C)) {
            Time.timeScale += 0.5f;
            Debug.Log($"Current game speed is {Time.timeScale}");
        }
        
        if (Input.GetKeyDown(KeyCode.X)) {
            Time.timeScale -= 0.5f;
            Debug.Log($"Current game speed is {Time.timeScale}");
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            if (currentAction == 0) {
                MoveSelection();
            } else if (currentAction == 1) {
                PlayerBag();
            }  else if (currentAction == 2) {
                PlayerPokemon();
            }  else if (currentAction == 3) {
                StartCoroutine(RunTurns(BattleAction.RUN));
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
        Action onSelected = () => {
            var selectedMember = partyScreen.SelectedMember;

            if (selectedMember.CurrentHitpoints <= 0) {
                partyScreen.SetMessageText("You can't send out a fainted pokemon!");
                return;
            }
            if (selectedMember == playerUnit.Pokemon){
                partyScreen.SetMessageText("You can't switch with the same pokemon!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ACTION_SELECTION){
                StartCoroutine(RunTurns(BattleAction.SWITCH_POKEMON));
            } else { // pokemon fainted
                state = BattleState.BUSY;
                bool isTrainerSwitching = partyScreen.CalledFrom == BattleState.SWITCHING;

                StartCoroutine(SwitchPokemon(selectedMember, isTrainerSwitching));
            }
            
            partyScreen.CalledFrom = null;
        };

        Action onBack = () => {
            if (playerUnit.Pokemon.CurrentHitpoints <= 0) {
                partyScreen.SetMessageText("Choose your next pokemon.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.SWITCHING) {
                StartCoroutine(SendNextTrainerPokemon());
            } else {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleSwitching() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
            switchingChoice = !switchingChoice;
        }

        dialogueBox.UpdateChoiceBox(switchingChoice);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            dialogueBox.EnableChoiceBox(false);

            if (switchingChoice) {               
                PlayerPokemon();
                partyScreen.SetMessageText("Choose your next pokemon.");
            } else {
                StartCoroutine(SendNextTrainerPokemon());
            }
        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            dialogueBox.EnableChoiceBox(false);

            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerSwitching=false) {
        if (playerUnit.Pokemon.CurrentHitpoints > 0) {
            dialogueBox.EnableActionSelector(false);
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Blueprint.PokemonName}!");

            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        dialogueBox.EnableActionSelector(false);
        playerUnit.Setup(newPokemon);
        dialogueBox.SetMoveNames(newPokemon.Moves);

        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Blueprint.PokemonName}!");

        if (isTrainerSwitching) {
            StartCoroutine(SendNextTrainerPokemon());
        } else {
            state = BattleState.RUNNING_TURN;
        }       
    }

    IEnumerator SendNextTrainerPokemon() {
        state = BattleState.BUSY;

        var nextPokemon = trainerParty.GetLeadPokemon();

        enemyUnit.Setup(nextPokemon);
        yield return dialogueBox.TypeDialogue($"{trainer.TrainerName} sends out {nextPokemon.Blueprint.PokemonName}!");

        state = BattleState.RUNNING_TURN;
    }

    IEnumerator ThrowPokeball() {
        state = BattleState.BUSY;
        
        if (isTrainerbattle) {
            yield return dialogueBox.TypeDialogue($"You can't steal another trainer's pokemon!");
            state = BattleState.RUNNING_TURN;
            yield break;
        }

        yield return dialogueBox.TypeDialogue($"{player.PlayerName} used a Pokeball!");

        var pokeballObject = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity);
        var pokeball = pokeballObject.GetComponent<SpriteRenderer>();

        // Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0.0f, 2.0f, 0.0f), 2.0f, 1, 1.0f).WaitForCompletion();
        yield return enemyUnit.PlayCatchAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++){
            yield return new WaitForSeconds(0.5f);
            pokeball.transform.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), 0.3f).WaitForCompletion();
            yield return new WaitForSeconds(1.0f);
        }
        
        if (shakeCount == 4) {
            // pokemon is caught
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.PokemonName} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();
            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.PokemonName} was added to the party!");

            Destroy(pokeball);
            BattleOver(true);
        } else {
            // pokemon escapes
            yield return new WaitForSeconds(1.0f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2){
                yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.PokemonName} broke free!");
            } else {
                yield return dialogueBox.TypeDialogue($"{enemyUnit.Pokemon.Blueprint.PokemonName} was almost caught!");
            }

            Destroy(pokeball);
            state = BattleState.RUNNING_TURN;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon) {
        float a = (3 * pokemon.MaxHitpoints - 2 * pokemon.CurrentHitpoints) * pokemon.Blueprint.CatchRate * ConditionsDataBase.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHitpoints);
        if (a >= 255) {
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4) {
            if (UnityEngine.Random.Range(0, 65535) >= b) {
                break;
            }
            ++shakeCount;
        }
        return shakeCount;
    }

    IEnumerator TryToEscape() {
        state = BattleState.BUSY;

        if (isTrainerbattle) {
            yield return dialogueBox.TypeDialogue("You can't run from trainer battles!");

            state = BattleState.RUNNING_TURN;

            yield break;
        }

        runAttempts++;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed) {
            yield return dialogueBox.TypeDialogue("Got away safely!");

            BattleOver(true);
        } else {
            float runChance = playerSpeed * 128 / enemySpeed + 30 * runAttempts;

            if (UnityEngine.Random.Range(0, 256) < runChance) {
                yield return dialogueBox.TypeDialogue("Got away safely!");

                BattleOver(true);
            } else {
                yield return dialogueBox.TypeDialogue("Failed to escape!");

                state = BattleState.RUNNING_TURN;
            }
        }
    }
}
