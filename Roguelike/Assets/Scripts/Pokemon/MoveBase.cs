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

    public string GetDescription() {
        return description;
    }
}
