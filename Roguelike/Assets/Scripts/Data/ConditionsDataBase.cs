using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDataBase {

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = 
                                        new Dictionary<ConditionID, Condition> {
        {
            ConditionID.POISON, new Condition() {
                Name = " Poison",
                StartMessage = "has been poisoned!",
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHitpoints(pokemon.MaxHitpoints / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} was hurt by the poison!");
                }
            }
        },
        {
            ConditionID.BURN, new Condition() {
                Name = " Poison",
                StartMessage = "has been Burned!",
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHitpoints(pokemon.MaxHitpoints / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Blueprint.PokemonName} was hurt by the burn!");
                }
            }
        }
    };
}

public enum ConditionID {
    NONE, POISON, BURN, SLEEP, PARALYSIS, FREEZE
}
