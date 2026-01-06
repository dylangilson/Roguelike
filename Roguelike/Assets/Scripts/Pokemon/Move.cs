using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
    public MoveBase Blueprint { get; set; }
    public int PowerPoints { get; set; }

    public Move(MoveBase moveBase) {
        Blueprint = moveBase;
        PowerPoints = moveBase.PowerPoints;
    }

    public Move(MoveSaveData saveData) {
        Blueprint = MoveDataBase.GetMoveByName(saveData.name);
        PowerPoints = saveData.powerPoints;
    }

    public MoveSaveData GetSaveData() {
        var saveData = new MoveSaveData() {
            name = Blueprint.MoveName,
            powerPoints = PowerPoints
        };

        return saveData;
    }

    public bool IncreasePowerPoints(int amount) {
        if (PowerPoints == Blueprint.PowerPoints) {
            return false;
        }

        PowerPoints = Mathf.Clamp(PowerPoints + amount, 0, Blueprint.PowerPoints);

        return true;
    }
}

[Serializable]
public class MoveSaveData {
    public string name;
    public int powerPoints;
}
