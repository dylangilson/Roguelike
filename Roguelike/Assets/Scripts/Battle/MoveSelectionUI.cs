using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour {
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color hightlightColour;

    int index = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove) {
        for (int i = 0; i < currentMoves.Count; i++) {
            moveTexts[i].text = currentMoves[i].MoveName;
        }

        moveTexts[currentMoves.Count].text = newMove.MoveName;
    }

    public void HandleMoveSelection(Action<int> onSelected) {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            index++;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            index--;
        }

        index = Mathf.Clamp(index, 0, PokemonBase.MAX_NUMBER_OF_MOVES);

        UpdateMoveSelection(index);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
            onSelected?.Invoke(index);
        }
    }

    public void UpdateMoveSelection(int index) {
        for (int i = 0; i < PokemonBase.MAX_NUMBER_OF_MOVES + 1; i++) {
            if (i == index) {
                moveTexts[i].color = hightlightColour;
            } else {
                moveTexts[i].color = Color.black;
            }
        }
    }
}
