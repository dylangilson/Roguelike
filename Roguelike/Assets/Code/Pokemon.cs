using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type {
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

[System.Serializable]
public class Move : MonoBehaviour {
    public string moveName;
    public Type type;
    public int power;
    public int accuracy;
    // TODO: public MoveEffect effect;

    public Move(string name, Type type, int power, int accuracy) {
        this.moveName = name;
        this.type = type;
        this.power = power;
        this.accuracy = accuracy;
    }
}

public class Stats : MonoBehaviour {
    public int hp;
    public int attack;
    public int specialAttack;
    public int defence;
    public int specialDefence;
    public int speed;

    public Stats(int hp, int attack, int specialAttack, int defence, int specialDefence, int speed) {
        this.hp = hp;
        this.attack = attack;
        this.specialAttack = specialAttack;
        this.defence = defence;
        this.specialDefence = specialDefence;
        this.speed = speed;
    }
}

public class Learnset : MonoBehaviour {
    // NOTE: both lists MUST have the same number of elements, where level[i] dictates at what level move[i] is learned
    public List<Move> moves;
    public List<int> levels;

    public Learnset() {
        this.moves = new List<Move>();
        this.levels = new List<int>();
    }

    public void AddMove(Move move, int level) {
        moves.Add(move);
        levels.Add(level);
    }

    public Move CheckMoveOnLevelUp(int currentLevel) {
        for (int i = 0; i < levels.Count; i++) {
            if (levels[i] == currentLevel) {
                return moves[i];
            }

            if (levels[i] > currentLevel) {
                break;
            }
        }

        return null;
    }

    public void ListLearnset() {
        Debug.Log("Learnset:");
        for (int i = 0; i < moves.Count; i++) {
            Debug.Log($"{moves[i].name} at level {levels[i]}");
        }
    }
}

public class Pokemon : MonoBehaviour {
    public string pokemonName;
    public Type primaryType;
    public Type? secondaryType;
    public int maxHP;
    public int currentHP;
    public List<Move> moves;
    public int level;
    public Item heldItem;
    public Stats stats;
    public Learnset learnset;

    public Pokemon(string name, Type primaryType, Type? secondaryType, int maxHP, List<Move> moves, int level, Item heldItem, Stats stats, Learnset learnset) {
        this.pokemonName = name;
        this.primaryType = primaryType;
        this.secondaryType = secondaryType;
        this.maxHP = maxHP;
        this.currentHP = maxHP;
        this.moves = moves;
        this.level = level;
        this.heldItem = heldItem;
        this.stats = stats;
        this.learnset = learnset;
    }

    public void LevelUp() {
        level++;
        Debug.Log($"{pokemonName} leveled up to level {level}!");

        Move move = learnset.CheckMoveOnLevelUp(level);

        if (!moves.Contains(move)) {
            LearnMove(move);
        }
    }

    public void LearnMove(Move move) {
        if (moves.Count >= 4) {
            Debug.Log($"{pokemonName} cannot learn more than 4 moves. Please forget a move to learn {move.moveName}.");
            // TODO: forget move / cancel move learn (needs UI)
        } else {
            moves.Add(move);
            Debug.Log($"{pokemonName} learned {move.moveName}!");
        }
    }

    public bool TakeDamage(int damage) {
        currentHP -= damage;

        if (currentHP < 0) {
            currentHP = 0;
        }

        Debug.Log($"{pokemonName} took {damage} damage. Current HP: {currentHP}");

        return currentHP == 0 ? true : false;
    }

    public void UseMove(int moveIndex, Pokemon target) {
        if (moveIndex < 0 || moveIndex >= moves.Count) {
            Debug.Log("Invalid move index.");
            return;
        }

        Move move = moves[moveIndex];
        Debug.Log($"{pokemonName} used {move.moveName}!");

        // Calculate if the move hits based on its accuracy
        if (Random.Range(0, 100) < move.accuracy) {
            Debug.Log($"{move.moveName} hits!");
            target.TakeDamage(move.power);
        }
        else {
            Debug.Log($"{move.moveName} missed!");
        }
    }

    public void DisplayStatus() {
        Debug.Log($"Name: {pokemonName}, Type: {primaryType}{(secondaryType.HasValue ? $"/{secondaryType.Value}" : "")}, HP: {currentHP}/{maxHP}");
    }

    public void Heal(int amount) {
		currentHP += amount;

		if (currentHP > maxHP) {
			currentHP = maxHP;
        }
	}
}
