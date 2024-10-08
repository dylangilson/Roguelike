using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    
    PartyMemberUI[] memberSlots;

    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Pokemon> pokemon) {
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemon.Count) {
                memberSlots[i].SetData(pokemon[i]);
            } else if (i >= pokemon.Count) {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose A Pokémon!";
    }
}
