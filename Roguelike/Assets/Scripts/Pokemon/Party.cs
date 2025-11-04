using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Party : MonoBehaviour {
    [SerializeField] List<Pokemon> party;

    public event Action OnUpdated;

    public List<Pokemon> GetParty() {
        return party;
    }

    public void SetParty(List<Pokemon> pokemon) {
        party = pokemon;

        OnUpdated?.Invoke();
    }
    
    private void Start() {
        foreach (var pokemon in party) {
            pokemon.Init();
        }
    }

    public Pokemon GetLeadPokemon() {
        for (int i = 0; i < party.Count; i++) {
            if (party[i].CurrentHitpoints > 0) {
                return party[i];
            }
        }

        return null;
    }

    public void AddPokemon(Pokemon pokemon){
        if (party.Count < 6) {
            party.Add(pokemon);
            OnUpdated?.Invoke();
        } else {
            // TODO: add to PC 
        }
    }

    public static Party GetPlayerParty() {
        return FindObjectOfType<PlayerController>().GetComponent<Party>();
    }
}
