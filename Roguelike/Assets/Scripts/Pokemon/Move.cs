using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
    public MoveBase Blueprint { get; set; }
    public int PowerPoints { get; set; }

    public Move(MoveBase pokemonBase) {
        Blueprint = pokemonBase;
        PowerPoints = pokemonBase.PowerPoints;
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
}

[Serializable]
public class MoveSaveData {
    public string name;
    public int powerPoints;
}
