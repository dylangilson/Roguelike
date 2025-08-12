using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDataBase {
    static Dictionary<string, MoveBase> moves;

    public static void Init() {
        moves = new Dictionary<string, MoveBase>();
        var moveBases = Resources.LoadAll<MoveBase>("");

        foreach (var moveBase in moveBases) {
            if (moves.ContainsKey(moveBase.MoveName)) {
                Debug.LogError($"There are two MoveBase files with the name {moveBase.MoveName}");

                continue;
            }

            moves[moveBase.MoveName] = moveBase;
        }
    }

    public static MoveBase GetMoveByName(string name) {
        if (!moves.ContainsKey(name)) {
            Debug.LogError($"Move with the name {name} not found in MoveDataBase");

            return null;
        }

        return moves[name];
    }
}
