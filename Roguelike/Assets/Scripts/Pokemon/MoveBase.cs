using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create New Move")]
public class MoveBase : ScriptableObject {
    [SerializeField] string moveName;
    [SerializeField] Type moveType;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int powerPoints;
    [SerializeField] MoveCatagory moveCatagory;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;

    [TextArea]
    [SerializeField] string description;

    public string GetMoveName() {
        return moveName;
    }

    public Type GetMoveType() {
        return moveType;
    }

    public int GetPower() {
        return power;
    }

    public int GetAccuracy() {
        return accuracy;
    }

    public int GetPowerPoints() {
        return powerPoints;
    }

    public MoveCatagory GetMoveCatagory() {
        return moveCatagory;
    }

    public MoveEffects GetMoveEffects() {
        return effects;
    }

    public MoveTarget GetMoveTarget() {
        return target;
    }

    public string GetDescription() {
        return description;
    }
}

[System.Serializable]
public class StatBoost {
    public Stat stat;
    public int boost;
}

[System.Serializable]
public class MoveEffects {
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> GetBoosts() {
        return boosts;
    }
}

public enum MoveTarget {
    FOE,
    SELF
}