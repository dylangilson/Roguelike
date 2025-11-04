using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon {
    [SerializeField] PokemonBase blueprint;
    [SerializeField] int level;

    public Pokemon(PokemonBase copyBlueprint, int copyLevel) {
        blueprint = copyBlueprint;
        level = copyLevel;
        Init();
    }

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

    public int Exp { get; set; }
    public int CurrentHitpoints { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }    
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public int MaxHitpoints { get; private set; }
    public Queue<string> StatusChanges { get; private set; }
    public Condition Status { get; private set; }
    public int StatusCounter { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusCounter { get; set; }
    public bool HitpointsChanged { get; set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHitpointsChanged;

    public void Init() {
        // generate moves
        Moves = new List<Move>();

        for (int i = Blueprint.LearnableMoves.Count - 1; i >= 0; i--) {
            var move = Blueprint.LearnableMoves[i];

            if (move.GetLevel() <= Level) {
                Moves.Add(new Move(move.GetBase()));
            }

            if (Moves.Count >= PokemonBase.MAX_NUMBER_OF_MOVES) {
                break;
            }
        }

        Exp = Blueprint.GetExp(Level);

        CalculateStats();
        
        CurrentHitpoints = MaxHitpoints;
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData) {
        // reload data from savefile
        blueprint = PokemonDataBase.GetPokemonByName(saveData.name);
        level = saveData.level;
        Exp = saveData.exp;
        CurrentHitpoints = saveData.hitpoints;

        if (saveData.statusID != null) {
            Status = ConditionsDataBase.Conditions[saveData.statusID.Value];
        } else {
            Status = null;
        }

        Moves = saveData.moves.Select(move => new Move(move)).ToList();

        // initialize other properties of Pokemon
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData() {
        var saveData = new PokemonSaveData() {
            name = blueprint.PokemonName,
            level = Level,
            exp = Exp,
            hitpoints = CurrentHitpoints,
            statusID = Status?.ID,
            moves = Moves.Select(move => move.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.ATTACK, Mathf.FloorToInt((Blueprint.Attack * Level) / 100.0f) + 5);
        Stats.Add(Stat.DEFENCE, Mathf.FloorToInt((Blueprint.Defence * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPECIALATTACK, Mathf.FloorToInt((Blueprint.SpecialAttack * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPECIALDEFENCE, Mathf.FloorToInt((Blueprint.SpecialDefence * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPEED, Mathf.FloorToInt((Blueprint.Speed * Level) / 100.0f) + 5);

        MaxHitpoints = Mathf.FloorToInt((Blueprint.Hitpoints * Level) / 100.0f) + 10 + Level;
    }

    void ResetStatBoosts() {
        StatBoosts = new Dictionary<Stat, int>() {
            { Stat.ATTACK, 0 },
            { Stat.DEFENCE, 0 },
            { Stat.SPECIALATTACK, 0 },
            { Stat.SPECIALDEFENCE, 0 },
            { Stat.SPEED, 0 },
            { Stat.ACCURACY, 0 },
            { Stat.EVASION, 0 }
        };
    }

    int GetStat(Stat stat) {
        int statValue = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] {1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f};

        if (boost >= 0) {
            statValue = Mathf.FloorToInt(statValue * boostValues[boost]);
        } else if (boost < 0) {
            statValue = Mathf.FloorToInt(statValue / boostValues[-boost]);
        }

        return statValue;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts) {
        foreach (var statBoost in statBoosts) {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0) {
                StatusChanges.Enqueue($"{Blueprint.PokemonName}'s {stat} rose!");
            } else if (boost < 0) {
                StatusChanges.Enqueue($"{Blueprint.PokemonName}'s {stat} fell!");
            }

            Debug.Log($"{Blueprint.PokemonName}'s {stat} has been altered!");
        }
    }

    public bool CheckForLevelUp() {
        int nextLevelExpThreshold = Blueprint.GetExp(level + 1);

        if (Exp > nextLevelExpThreshold) {
            level++;
        
            return true;
        }

        return false;
    }

    // get new move or return null
    public LearnableMove GetLearnableMoveAtCurrentLevel() {
        return Blueprint.LearnableMoves.Where(x => x.GetLevel() == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn) {
        if (Moves.Count >= PokemonBase.MAX_NUMBER_OF_MOVES) {
            return;
        }

        Moves.Add(new Move(moveToLearn.GetBase()));
    }

    public List<string> GetMoveNames() {
        List<string> moveNames = new List<string>();
        
        foreach (Move move in Moves) {
            moveNames.Add(move.Blueprint.MoveName);
        }
        
        return moveNames;
    }

    public int Attack {
        get { return GetStat(Stat.ATTACK); }
    }

    public int Defence {
        get { return GetStat(Stat.DEFENCE); }
    }

    public int SpecialAttack {
        get { return GetStat(Stat.SPECIALATTACK); }
    }

    public int SpecialDefence {
        get { return GetStat(Stat.SPECIALDEFENCE); }
    }

    public int Speed {
        get { return GetStat(Stat.SPEED); }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker) {
        float critical = 1.0f;

        if (Random.value * 100.0f <= 6.25f) {
            critical = 2.0f;
        }

        float typeAModifier = TypeChart.GetEffectiveness(move.Blueprint.MoveType, this.Blueprint.TypeA);
        float typeBModifier = TypeChart.GetEffectiveness(move.Blueprint.MoveType, this.Blueprint.TypeB);
        float typeModifier =  typeAModifier * typeBModifier;

        var damageDetails = new DamageDetails() {
            Effectiveness = typeModifier,
            Critical = critical,
            Fainted = false
        };

        float attack = 0;
        float defence = 0;

        if (move.Blueprint.MoveCatagory == MoveCatagory.PHYSICAL) {
            attack = attacker.Attack;
            Debug.Log($"{attacker.Blueprint.PokemonName}'s attack is {attacker.Attack}");

            defence = Defence;
        } else if (move.Blueprint.MoveCatagory == MoveCatagory.SPECIAL) {
            attack = attacker.SpecialAttack;
            Debug.Log($"{attacker.Blueprint.PokemonName}'s special attack is {attacker.SpecialAttack}");

            defence = SpecialDefence;
        } else {
            Debug.Log(move.Blueprint.MoveName + " is not Physical or Special!");
        }

        float modifiers = Random.Range (0.85f, 1.0f) * typeModifier * critical;
        float attackModifier = (2 * attacker.Level + 10) / 250.0f;
        float defenseModifier = attackModifier * move.Blueprint.Power * (attack / defence) + 2;
        int damage = Mathf.Clamp(Mathf.FloorToInt(defenseModifier * modifiers), 1, 100000);
        Debug.Log($"{attacker.Blueprint.PokemonName} deals {damage} much damage");

        DecreaseHitpoints(damage);

        return damageDetails;
    }

    public void DecreaseHitpoints(int damage) {
        CurrentHitpoints = Mathf.Clamp(CurrentHitpoints - damage, 0, MaxHitpoints);

        OnHitpointsChanged?.Invoke();

        HitpointsChanged = true;
    }

    public void IncreaseHitpoints(int amount) {
        CurrentHitpoints = Mathf.Clamp(CurrentHitpoints + amount, 0, MaxHitpoints);

        OnHitpointsChanged?.Invoke();

        HitpointsChanged = true;
    }

    public void SetStatus(ConditionID conditionID) {
        Status = ConditionsDataBase.Conditions[conditionID];
        bool success = false;

        if (Status?.OnStart != null) {
            success = Status.OnStart(this);
        }

        StatusChanges.Enqueue($"{Blueprint.PokemonName} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus() {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionID) {
        VolatileStatus = ConditionsDataBase.Conditions[conditionID];
        bool success = false;

        if (VolatileStatus?.OnStart != null) {
            success = VolatileStatus.OnStart(this);
        }

        StatusChanges.Enqueue($"{Blueprint.PokemonName} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus() {
        VolatileStatus = null;
    }

    public Move GetRandomMove() {
        var movesWithPP = Moves.Where(move => move.PowerPoints > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);

        return movesWithPP[r];
    }

    public bool OnBeforeMove() {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null) {
            if (!Status.OnBeforeMove(this)) {
                return false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null) {
            if (!VolatileStatus.OnBeforeMove(this)) {
                return false;
            }
        }

        return canPerformMove;
    }

    public Pokemon OnAfterTurn() {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
        return this;
    }

    public void OnBattleOver() {
        VolatileStatus = null;
        ResetStatBoosts();
    }
}

public class DamageDetails {
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Effectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData {
    public string name;
    public int level;
    public int exp;
    public int hitpoints;
    public ConditionID? statusID;
    public List<MoveSaveData> moves;
}
