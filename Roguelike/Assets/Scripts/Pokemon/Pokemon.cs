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
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public int MaxHitpoints { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public void Init() {
        Moves = new List<Move>();

        // generate moves
        for (int i = Blueprint.GetLearnableMoves().Count - 1; i >= 0; i--) {
            var move = Blueprint.GetLearnableMoves()[i];

            if (move.GetLevel() <= Level) {
                Moves.Add(new Move(move.GetBase()));
            }

            if (Moves.Count >= 4) {
                break;
            }
        }

        CalculateStats();
        
        CurrentHitpoints = MaxHitpoints;

        ResetStatBoosts();
    }

    void CalculateStats() {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.ATTACK, Mathf.FloorToInt((Blueprint.GetAttack() * Level) / 100.0f) + 5);
        Stats.Add(Stat.DEFENCE, Mathf.FloorToInt((Blueprint.GetDefence() * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPECIALATTACK, Mathf.FloorToInt((Blueprint.GetSpecialAttack() * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPECIALDEFENCE, Mathf.FloorToInt((Blueprint.GetSpecialDefence() * Level) / 100.0f) + 5);
        Stats.Add(Stat.SPEED, Mathf.FloorToInt((Blueprint.GetSpeed() * Level) / 100.0f) + 5);

        MaxHitpoints = Mathf.FloorToInt((Blueprint.GetHitpoints() * Level) / 100.0f) + 10;
    }

    void ResetStatBoosts() {
        StatBoosts = new Dictionary<Stat, int>() {
            {Stat.ATTACK, 0},
            {Stat.DEFENCE, 0},
            {Stat.SPECIALATTACK, 0},
            {Stat.SPECIALDEFENCE, 0},
            {Stat.SPEED, 0},
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
                StatusChanges.Enqueue($"{Blueprint.GetPokemonName()}'s {stat} rose!");
            } else if (boost < 0) {
                StatusChanges.Enqueue($"{Blueprint.GetPokemonName()}'s {stat} fell!");
            }

            Debug.Log($"{Blueprint.GetPokemonName()}'s {stat} has been altered!");
        }
    }

    public int GetAttack() {
        return GetStat(Stat.ATTACK);
    }

    public int GetDefence() {
        return GetStat(Stat.DEFENCE);
    }

    public int GetSpecialAttack() {
        return GetStat(Stat.SPECIALATTACK);
    }

    public int GetSpecialDefence() {
        return GetStat(Stat.SPECIALDEFENCE);
    }

    public int GetSpeed() {
        return GetStat(Stat.SPEED);
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

        if (move.Blueprint.GetMoveCatagory() == MoveCatagory.PHYSICAL) {
            attack = attacker.GetAttack();
            Debug.Log($"{attacker.Blueprint.GetPokemonName()}'s attack is {attacker.GetAttack()}");
            defence = GetDefence();
            // Debug.Log($"Defender's defence is {GetDefence()}");
        } else if (move.Blueprint.GetMoveCatagory() == MoveCatagory.SPECIAL) {
            attack = attacker.GetSpecialAttack();
            Debug.Log($"{attacker.Blueprint.GetPokemonName()}'s special attack is {attacker.GetSpecialAttack()}");
            defence = GetSpecialDefence();
            // Debug.Log($"Defender's special defence is {GetSpecialDefence()}");
        } else {
            Debug.Log(move.Blueprint.GetMoveName() + " is not Physical or Special!");
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

    public void OnBattleOver() {
        ResetStatBoosts();
    }
}

public class DamageDetails {
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Effectiveness { get; set; }
}