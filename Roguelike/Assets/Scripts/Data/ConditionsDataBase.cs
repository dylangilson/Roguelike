using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDataBase {
    public static void Init() {
        foreach (var key_value_pair in Conditions) {
            var conditionID = key_value_pair.Key;
            var condition = key_value_pair.Value;

            condition.ID = conditionID;
        }
    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition> {
        {
            ConditionID.POISON, new Condition() {
                Name = "Poison",
                Abbreviation = "PSN",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHitpoints(pokemon.MaxHitpoints / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} was hurt by the poison!");
                }
            }
        },
        {
            ConditionID.BURN, new Condition() {
                Name = "Burn",
                Abbreviation = "BRN",
                StartMessage = "has been burned!",
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHitpoints(pokemon.MaxHitpoints / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} was hurt by the burn!");
                }
            }
        },
        {
            ConditionID.PARALYSIS, new Condition() {
                Name = "Paralysis",
                Abbreviation = "PAR",
                StartMessage = "has been paralyzed!",
                OnBeforeMove = (Pokemon pokemon) => {
                    if (Random.Range(1, 5) == 1) {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is paralyzed!");

                        return false;
                    }
                    
                    return true;
                }
            }
        },
        {
            ConditionID.FREEZE, new Condition() {
                Name = "Freeze",
                Abbreviation = "FRZ",
                StartMessage = "has been frozen!",
                OnBeforeMove = (Pokemon pokemon) => {
                    if (Random.Range(1, 5) == 1) {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} has thawed!");

                        return true;
                    }
                    
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is frozen solid!");

                    return false;
                }
            }
        },
        {
            ConditionID.SLEEP, new Condition() {
                Name = "Sleep",
                Abbreviation = "SLP",
                StartMessage = "fell asleep!",
                OnStart = (Pokemon pokemon) => {
                    if (pokemon.StatusCounter > 0) {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is already asleep!");

                        return false;
                    }

                    pokemon.StatusCounter = Random.Range(1, 6);

                    return true;
                },
                OnBeforeMove = (Pokemon pokemon) => {
                    pokemon.StatusCounter--;
                    Debug.Log(pokemon.StatusCounter);

                    if (pokemon.StatusCounter == 0) {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} has woken up!");
                        
                        return true;
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is sleeping!");

                    return false;
                }
            }
        },

        // volatile status conditions
        {
            ConditionID.CONFUSION, new Condition() {
                Name = "Confusion",
                Abbreviation = "CON",
                StartMessage = "is confused!",
                OnStart = (Pokemon pokemon) => {
                    // confused for 1-4 turns
                    pokemon.VolatileStatusCounter = Random.Range(1, 5);
                    Debug.Log($"{pokemon.Blueprint.PokemonName} will be confused for {pokemon.VolatileStatusCounter} turns!");

                    return true;
                },
                OnBeforeMove = (Pokemon pokemon) => {
                    pokemon.VolatileStatusCounter--;
                    Debug.Log(pokemon.VolatileStatusCounter);

                    if (pokemon.VolatileStatusCounter == 0) {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is no longer confused!");
                        
                        return true;
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} is confused!");

                    // check to see if move performs anyways, 50% chance
                    if (Random.Range(1, 3) == 1){
                        return true;
                    }

                    // pokemon hurt itself
                    pokemon.UpdateHitpoints(pokemon.MaxHitpoints / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} hurt itself due to confusion, Fucking idiot!");

                    return false;
                }
            }
        }
    };
}

public enum ConditionID {
    NONE, POISON, BURN, SLEEP, PARALYSIS, FREEZE, 
    CONFUSION // volatile conditions
}
