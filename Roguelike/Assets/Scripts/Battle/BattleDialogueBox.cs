using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour {
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColour;
    [SerializeField] Text dialogueText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    public IEnumerator TypeDialogue(string dialogue) {
        dialogueText.text = "";

        foreach (var letter in dialogue.ToCharArray()) {
            dialogueText.text += letter;

            yield return new WaitForSeconds(1.0f / lettersPerSecond);
        }
        
        yield return new WaitForSeconds(1.0f);
    }

    public void EnableDialogueText(bool enabled) {
        dialogueText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled) {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled) {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction) {
        for (int i = 0; i < actionTexts.Count; i++) {
            if (i == selectedAction) {
                actionTexts[i].color = highlightedColour;
            } else {
                actionTexts[i].color = Color.black;
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move) {
        for (int i = 0; i < moveTexts.Count; i++) {
            if (i == selectedMove) {
                moveTexts[i].color = highlightedColour;
            } else {
                moveTexts[i].color = Color.black;
            }

            ppText.text = $"PP {move.PowerPoints} / {move.Blueprint.PowerPoints}";
            typeText.text = move.Blueprint.MoveType.ToString();

            if (move.PowerPoints == 0) {
                ppText.color = Color.red;
            } else if (move.PowerPoints <= Mathf.Ceil((float)move.Blueprint.PowerPoints / 3.0f)) {
                ppText.color = Color.yellow;
            } else {
                ppText.color = Color.black;
            }
        }
    }
    
    public void SetMoveNames(List<Move> moves) {
        for (int i = 0; i < moveTexts.Count; i++) {
            if (i < moves.Count) {
                moveTexts[i].text = moves[i].Blueprint.MoveName;
            } else {
                moveTexts[i].text = "-";
            }
        }
    }
}
