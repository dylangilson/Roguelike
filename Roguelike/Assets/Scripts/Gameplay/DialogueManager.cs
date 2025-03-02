using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Text dialogueText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialogue;
    public event Action OnCloseDialogue;

    private Dialogue dialogue;
    private int index; // index of current line
    private bool isTyping; // true -> dialogue box is typing letters per second

    public static DialogueManager Instance { get; private set; } // global instance of DialogueManager

    private void Awake() {
        Instance = this;
    }

    public IEnumerator ShowDialogue(Dialogue dialogue) {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke(); // change GameController state to DIALOGUE

        this.dialogue = dialogue;

        dialogueBox.SetActive(true); // display dialogue box

        StartCoroutine(TypeDialogue(dialogue.Lines[0])); // populate dialogue box
    }

    public void HandleUpdate() {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            if (isTyping) {
                dialogueText.text = "";
                dialogueText.text = dialogue.Lines[index];

                isTyping = false;
            } else {
                index++;

                if (index < dialogue.Lines.Count) {
                    StartCoroutine(TypeDialogue(dialogue.Lines[index])); // populate dialogue box
                } else {
                    index = 0;

                    dialogueBox.SetActive(false); // stop display of dialogue box

                    OnCloseDialogue?.Invoke();
                }
            }
        }
    }

    private IEnumerator TypeDialogue(string line) {
        isTyping = true;

        dialogueText.text = "";

        foreach (var letter in line.ToCharArray()) {
            if (isTyping) {
                dialogueText.text += letter;

                yield return new WaitForSeconds(1.0f / lettersPerSecond);
            }
        }

        isTyping = false;
    }
}
