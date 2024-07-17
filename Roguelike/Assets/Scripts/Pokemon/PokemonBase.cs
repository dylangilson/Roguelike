using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create New Pokemon")]

public class PokemonBase : ScriptableObject {
    [SerializeField] string pokemonName;

    [TextArea]
    [SerializeField] string description;
    
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Type typeA;
    [SerializeField] Type typeB;
    [SerializeField] List<LearnableMove> learnableMoves;

    // base stats
    [SerializeField] int maxHitpoints;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefence;
    [SerializeField] int speed;    

    public string GetPokemonName() {
        return pokemonName;
    }

    public string GetDescription() {
        return description;
    }
    
    public Sprite GetFrontSprite() {
        return frontSprite;
    }

    public Sprite GetBackSprite() {
        return backSprite;
    }

    public Type GetTypeA() {
        return typeA;
    }

    public Type GetTypeB() {
        return typeB;
    }

    public List<LearnableMove> GetLearnableMoves() {
        return learnableMoves;
    }

    public int GetMaxHitpoints() {
        return maxHitpoints;
    }

    public int GetAttack() {
        return attack;
    }

    public int GetDefence() {
        return defence;
    }

    public int GetSpecialAttack() {
        return specialAttack;
    }

    public int GetSpecialDefence() {
        return specialDefence;
    }

    public int GetSpeed() {
        return speed;
    }
}

[System.Serializable]
public class LearnableMove {
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase GetBase() {
        return moveBase;
    }

    public int GetLevel() {
        return level;
    }    
}

public enum Type {
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
