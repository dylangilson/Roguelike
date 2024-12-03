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
}
