using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon {
    [SerializeField] PokemonBase blueprint;
    [SerializeField] int level;

    public PokemonBase Blueprint { 
        get {
            return blueprint;
        }
    }
    public int Level { 
        get {
            return level;
        } 
    }
    public int CurrentHitpoints { get; set; }
    public List<Move> Moves { get; set; }

    public void Init() {
        CurrentHitpoints = GetHitpoints();

        // generate Moves
        Moves = new List<Move>();

        // TODO: delete this if our solution works
        // foreach (var move in Blueprint.GetLearnableMoves()) {
        //     if (move.GetLevel() <= Level) {
        //         Moves.Add(new Move(move.GetBase()));
        //     }

        //     if (Moves.Count >= 4) {
        //         break;
        //     }
        // }

        for (int i = Blueprint.GetLearnableMoves().Count - 1; i >= 0; i--) {
            var move = Blueprint.GetLearnableMoves()[i];
            if (move.GetLevel() <= Level) {
                Moves.Add(new Move(move.GetBase()));
            }

            if (Moves.Count >= 4) {
                break;
            }
        }
    }

    public int GetHitpoints() {
        return Mathf.FloorToInt((Blueprint.GetHitpoints() * Level) / 100.0f) + 10;
    }

    public int GetAttack() {
        return Mathf.FloorToInt((Blueprint.GetAttack() * Level) / 100.0f) + 5;
    }

    public int GetDefence() {
        return Mathf.FloorToInt((Blueprint.GetDefence() * Level) / 100.0f) + 5;
    }

    public int GetSpecialAttack() {
        return Mathf.FloorToInt((Blueprint.GetSpecialAttack() * Level) / 100.0f) + 5;
    }

    public int GetSpecialDefence() {
        return Mathf.FloorToInt((Blueprint.GetSpecialDefence() * Level) / 100.0f) + 5;
    }

    public int GetSpeed() {
        return Mathf.FloorToInt((Blueprint.GetSpeed() * Level) / 100.0f) + 5;
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker) {
        float critical = 1.0f;
        if (Random.value * 100.0f <= 6.25f) {
            critical = 2.0f;
        }
        float typeAModifier = TypeChart.GetEffectiveness(move.Blueprint.GetMoveType(), this.Blueprint.GetTypeA());
        float typeBModifier = TypeChart.GetEffectiveness(move.Blueprint.GetMoveType(), this.Blueprint.GetTypeB());
        float typeModifier =  typeAModifier * typeBModifier;

        var damageDetails = new DamageDetails() {
            Effectiveness = typeModifier,
            Critical = critical,
            Fainted = false
        };
        float attack = 0;
        float defence = 0;

        if (move.Blueprint.GetMoveCatagory() == MoveCatagory.Physical) {
            attack = attacker.GetAttack();
            defence = GetDefence();
        } else if (move.Blueprint.GetMoveCatagory() == MoveCatagory.Special) {
            attack = attacker.GetSpecialAttack();
            defence = GetSpecialDefence();
        } else {
            Debug.Log(move.Blueprint.GetMoveName() + " is not Physical or Special");
        }

        float modifiers = Random.Range (0.85f, 1.0f) * typeModifier * critical;
        float attackModifier = (2 * attacker.Level + 10) / 250.0f;
        float defenseModifier = attackModifier * move.Blueprint.GetPower() * (attack / defence) + 2;
        int damage = Mathf.FloorToInt(defenseModifier * modifiers);

        CurrentHitpoints -= damage;
        if (!(CurrentHitpoints > 0)) {
            CurrentHitpoints = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove() {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDetails {
    public bool Fainted { get; set; }

    public float Critical { get; set; }

    public float Effectiveness { get; set; }
}