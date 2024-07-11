using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]

public class PokemonBase : ScriptableObject {
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;
    
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base Stats
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefence;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;
    public string GetName() {
        return name;
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

    public PokemonType GetType1() {
        return type1;
    }

    public PokemonType GetType2() {
        return type2;
    }

    public int GetMaxHP() {
        return maxHP;
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

    public List<LearnableMove> GetLearnableMoves() {
        return learnableMoves;
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

public enum PokemonType {
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
