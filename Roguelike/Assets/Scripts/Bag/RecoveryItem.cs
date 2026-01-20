using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create New Recovery Item")]
public class RecoveryItem : ItemBase {
    [Header("Hitpoints")]
    [SerializeField] int amountHealed;
    [SerializeField] bool restoreMaxHitpoints;

    [Header("PowerPoints")]
    [SerializeField] int powerPointsRecovered;
    [SerializeField] bool restoreMaxPowerPoints;

    [Header("Condition")]
    [SerializeField] ConditionID statusCure;
    [SerializeField] bool cureAll;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon) {
        // revive
        if (revive || maxRevive) {
            if (pokemon.CurrentHitpoints > 0) {
                return false;
            }

            if (revive) {
                pokemon.IncreaseHitpoints(pokemon.MaxHitpoints / 2);
            } else if (maxRevive) {
                pokemon.IncreaseHitpoints(pokemon.MaxHitpoints);
            }

            pokemon.CureStatus();

            return true;
        }

        // can't use other recovery items on fainted pokemon
        if (pokemon.CurrentHitpoints == 0) {
            return false;
        }
        
        // potion
        if (restoreMaxHitpoints || amountHealed > 0) {
            if (pokemon.CurrentHitpoints == pokemon.MaxHitpoints) {
                return false;
            }

            if (restoreMaxHitpoints) {
                pokemon.IncreaseHitpoints(pokemon.MaxHitpoints);
            } else {
                pokemon.IncreaseHitpoints(amountHealed);
            }

            return true;
        }

        // status
        if (cureAll || statusCure != ConditionID.NONE) {
            if (pokemon.Status == null && pokemon.VolatileStatus == null) {
                return false;
            }

            if (cureAll) {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            } else {
                if (pokemon.Status != null && pokemon.Status.ID == statusCure) {
                    pokemon.CureStatus();
                } else if (pokemon.VolatileStatus != null && pokemon.VolatileStatus.ID == statusCure) {
                    pokemon.CureVolatileStatus();
                } else {
                    return false;
                }
            }

            return true;
        }

        // powerpoints
        if (restoreMaxPowerPoints || powerPointsRecovered > 0) {
            bool increasedMove = false;

            foreach (var move in pokemon.Moves) {
                bool increased = restoreMaxPowerPoints ? move.IncreasePowerPoints(move.Blueprint.PowerPoints) : move.IncreasePowerPoints(powerPointsRecovered);

                if (increased) {
                    increasedMove = true;
                }
            }

            return increasedMove;
        }        

        return false;
    }
}
