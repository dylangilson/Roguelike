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

    public string MoveName {
        get { return moveName; }
    }

    public Type MoveType {
        get { return moveType; }
    }

    public int Power {
        get { return power; }
    }

    public int Accuracy {
        get { return accuracy; }
    }

    public int PowerPoints {
        get { return powerPoints; }
    }

    public MoveCatagory MoveCatagory {
        get { return moveCatagory; }
    }

    public MoveEffects MoveEffects {
        get { return effects; }
    }

    public MoveTarget MoveTarget {
        get { return target; }
    }

    public string Description {
        get { return description; }
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
    [SerializeField] ConditionID status;

    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionID Status {
        get { return status; }
    }
}

public enum MoveTarget {
    FOE,
    SELF
}