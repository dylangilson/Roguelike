using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon {
    public PokemonBase Blueprint { get; set; }
    public int Level { get; set; }
    public int CurrentHitpoints { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemon(PokemonBase pokemonBase, int pokemonLevel) {
        Blueprint = pokemonBase;
        Level = pokemonLevel;
        CurrentHitpoints = GetMaxHitpoints();

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

    public int GetMaxHitpoints() {
        return Mathf.FloorToInt((Blueprint.GetMaxHitpoints() * Level) / 100.0f) + 10;
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

    public bool TakeDamage(Move move, Pokemon attacker) {
        float modifiers = Random.Range (0.85f, 1.0f);
        float attackModifier = (2 * attacker.Level + 10) / 250.0f;
        float defenseModifier = attackModifier * move.Blueprint.GetPower() * ((float) attacker.GetAttack() / GetDefence()) + 2;
        int damage = Mathf.FloorToInt(defenseModifier * modifiers);

        CurrentHitpoints -= damage;
        if (!(CurrentHitpoints > 0)) {
            CurrentHitpoints = 0;
            return true;
        }

        return false;
    }

    public Move GetRandomMove() {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}
