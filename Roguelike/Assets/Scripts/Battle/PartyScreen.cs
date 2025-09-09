using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour {
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemon;
    int selection = 0;

    public Pokemon SelectedMember {
        get {
            return pokemon[selection];
        } 
    }

    public BattleState? CalledFrom { get; set; } // party screen can be called from states: ACTION_SELECTION, RUNNING_TURN, SWITCHING

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true); // true -> include inactive party members
    }

    public void SetPartyData(List<Pokemon> pokemon) {
        this.pokemon = pokemon;

        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemon.Count) {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemon[i]);
            } else if (i >= pokemon.Count) {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);
        
        messageText.text = "Choose A Pokémon!";
    }

    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < pokemon.Count; i++) {
            if (i == selectedMember) {
                memberSlots[i].SetSelected(true);
            } else if (i != selectedMember) {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message) {
        messageText.text = message;
    }

    public void HandleUpdate(Action onSelected, Action onBack) {
        var previousSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++selection;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --selection;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            selection -= 2;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            selection += 2;
        }

        selection = Mathf.Clamp(selection, 0, pokemon.Count - 1);


        if (selection != previousSelection) {
            UpdateMemberSelection(selection);
        }
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            onSelected?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            onBack?.Invoke();
        }
    }
}
