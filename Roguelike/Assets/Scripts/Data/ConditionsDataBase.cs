using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDataBase {
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition> {
        {
            ConditionID.POISON, new Condition() {
                Name = "Poison",
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
        }
    };
}

public enum ConditionID {
    NONE, POISON, BURN, SLEEP, PARALYSIS, FREEZE
}
