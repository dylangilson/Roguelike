using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour {
    [SerializeField] Text messageText;
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemon;

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
        
        messageText.text = "Choose A Pokémon!";
    }

    public void UpdateMemberSelect(int selectedMember) {
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
}
