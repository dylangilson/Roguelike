using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
    public MoveBase blueprint { get; set; }
    public int powerPoints { get; set; }

    public Move(MoveBase pokemonBase) {
        blueprint = pokemonBase;
        powerPoints = pokemonBase.GetPowerPoints();
    }
}
