using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBase _base;
    int level;

    public int HP { get; set; }

    public List<Move> Moves { get; set; }
    public Pokemon(PokemonBase pokemonBase, int pokemonLevel) {
        _base = pokemonBase;
        level = pokemonLevel;
        HP = _base.GetMaxHP();

        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in _base.GetLearnableMoves()) {
            if (move.GetLevel() <= level){
                Moves.Add(new Move(move.GetBase()));
            }

            if (Moves.Count >= 4) {
                break;
            }
        }
    }

    public int GetMaxHP() {
        return Mathf.FloorToInt((_base.GetMaxHP() * level) / 100f) + 10;
    }

    public int GetAttack() {
        return Mathf.FloorToInt((_base.GetAttack() * level) / 100f) + 5;
    }

    public int GetDefence() {
        return Mathf.FloorToInt((_base.GetDefence() * level) / 100f) + 5;
    }

    public int GetSpecialAttack() {
        return Mathf.FloorToInt((_base.GetSpecialAttack() * level) / 100f) + 5;
    }

    public int GetSpecialDefence() {
        return Mathf.FloorToInt((_base.GetSpecialDefence() * level) / 100f) + 5;
    }

    public int GetSpeed() {
        return Mathf.FloorToInt((_base.GetSpeed() * level) / 100f) + 5;
    }
}
