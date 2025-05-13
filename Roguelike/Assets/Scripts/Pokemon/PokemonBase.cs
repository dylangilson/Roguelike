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
    [SerializeField] int catchRate = 255;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    // base stats
    [SerializeField] int hitpoints;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int specialAttack;
    [SerializeField] int specialDefence;
    [SerializeField] int speed;

    public string PokemonName {
        get { return pokemonName; }
    }

    public string Description {
        get { return description; }
    }
    
    public Sprite FrontSprite {
        get { return frontSprite; }
    }

    public Sprite BackSprite {
        get { return backSprite; }
    }

    public Type TypeA {
        get { return typeA; }
    }

    public Type TypeB {
        get { return typeB; }
    }

    public List<LearnableMove> LearnableMoves {
        get { return learnableMoves; }
    }

    public int CatchRate {
        get { return catchRate; }
    }

    public int ExpYield {
        get { return expYield; }
    }

    public GrowthRate GrowthRate {
        get { return growthRate; }
    }

    public int Hitpoints {
        get { return hitpoints; }
    }

    public int Attack {
        get { return attack; }
    }

    public int Defence {
        get { return defence; }
    }

    public int SpecialAttack {
        get { return specialAttack; }
    }

    public int SpecialDefence {
        get { return specialDefence; }
    }

    public int Speed {
        get { return speed; }
    }

    public int GetExp(int level) {
        if (growthRate == GrowthRate.FAST) {
            return 4 * level * level * level / 5; // (4n^3)/5
        } else if (growthRate == GrowthRate.MEDIUM_FAST) {
            return level * level * level; // n^3
        } else if (growthRate == GrowthRate.MEDIUM_SLOW) {
            return 6 * level * level * level / 5 - 15 * level * level + 100 * level - 140; // (6n^3)/5 - 15n^2 + 100n - 140
        } else if (growthRate == GrowthRate.SLOW) {
            return 5 * level * level * level / 4; // (5n^3)/4
        }

        return -1;
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
    NONE,
    NORMAL,
    FIRE,
    WATER,
    ELECTRIC,
    GRASS,
    ICE,
    FIGHTING,
    POISON,
    GROUND,
    FLYING,
    PSYCHIC,
    BUG,
    ROCK,
    GHOST,
    DRAGON,
    DARK,
    STEEL
}

public enum Stat {
    ATTACK,
    DEFENCE,
    SPECIALATTACK,
    SPECIALDEFENCE,
    SPEED,

    // these are invisible stats
    ACCURACY,
    EVASION
}

public enum GrowthRate {
    FAST, MEDIUM_FAST, MEDIUM_SLOW, SLOW
}

public enum MoveCatagory { OTHER, PHYSICAL, SPECIAL }

public class TypeChart {
    static float[][] chart = {
        //                         NOR   FIR   WAT   ELE   GRA   ICE   FIG   POI   GRO   FLY   PSY   BUG   ROC   GHO   DRA   DAR   STE
        /* Normal */  new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.0f, 1.0f, 1.0f, 0.5f},
        /* Fire */    new float[] {1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 2.0f},
        /* Water */   new float[] {1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f},
        /* Electric */new float[] {1.0f, 1.5f, 2.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f},
        /* Grass */   new float[] {1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 2.0f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f},
        /* Ice */     new float[] {1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f},
        /* Fighting */new float[] {2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f, 0.5f, 0.5f, 2.0f, 0.0f, 1.0f, 2.0f, 2.0f},
        /* Poison */  new float[] {1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.0f},
        /* Ground */  new float[] {1.0f, 2.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.0f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f},
        /* Flying */  new float[] {1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f},
        /* Psychic */ new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.5f},
        /* Bug */     new float[] {1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 2.0f, 0.5f},
        /* Rock */    new float[] {1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f},
        /* Ghost */   new float[] {0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f},
        /* Dragon */  new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f},
        /* Dark */    new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f},
        /* Steel */   new float[] {1.0f, 0.5f, 0.5f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f},
    };

    public static float GetEffectiveness(Type attackType, Type defenceType) {
        if (attackType == Type.NONE || defenceType == Type.NONE) {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenceType - 1;

        return chart[row][col];
    }
}
