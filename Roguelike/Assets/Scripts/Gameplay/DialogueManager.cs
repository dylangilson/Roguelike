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

    public static DialogueManager Instance { get; private set; } // global instance of DialogueManager
    public bool IsShowing { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void CloseDialogue() {
        dialogueBox.SetActive(false);

        IsShowing = false;
        OnCloseDialogue?.Invoke();
    }

    // single frame of dialogue
    public IEnumerator ShowDialogueText(string text, bool waitForInput=true) {
        OnShowDialogue?.Invoke();
        IsShowing = true;

        dialogueBox.SetActive(true); // display dialogue box

        yield return TypeDialogue(text); // populate dialogue box

        if (waitForInput) {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space));
        }

        CloseDialogue();
    }

    // multiple frames of dialogue
    public IEnumerator ShowDialogue(Dialogue dialogue) {
        yield return new WaitForEndOfFrame();

        OnShowDialogue?.Invoke(); // change GameController state to DIALOGUE

        IsShowing = true;

        dialogueBox.SetActive(true); // display dialogue box

        foreach (var line in dialogue.Lines) {
            yield return TypeDialogue(line); // populate dialogue box

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space));
        }

        dialogueBox.SetActive(false);

        IsShowing = false;

        OnCloseDialogue?.Invoke();
    }

    public void HandleUpdate() {
        
    }

    private IEnumerator TypeDialogue(string line) {
        dialogueText.text = "";

        foreach (var letter in line.ToCharArray()) {
            dialogueText.text += letter;

            yield return new WaitForSeconds(1.0f / lettersPerSecond);
        }
    }
}
