using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour {
    private List<Pokemon> party;
    public int maxPartySize = 6;

    public Party() {
        party = new List<Pokemon>();
    }

    public bool AddPokemon(Pokemon pokemon) {
        if (party.Count >= maxPartySize) {
            Debug.Log("Party is full!");
            return false;
        }

        party.Add(pokemon);
        Debug.Log(pokemon.name + " was added to the party.");
        return true;
    }

    public bool RemovePokemon(Pokemon pokemon) {
        if (party.Contains(pokemon)) {
            party.Remove(pokemon);
            Debug.Log(pokemon.name + " was removed from the party.");
            return true;
        }

        Debug.Log(pokemon.name + " is not in the party.");
        return false;
    }

    public void ListParty() {
        Debug.Log("Party contains " + party.Count + " Pokémon:");
        foreach (var pokemon in party) {
            pokemon.DisplayStatus();
        }
    }

    public Pokemon GetPokemon(int index) {
        if (index < 0 || index >= party.Count) {
            Debug.Log("Invalid party index.");
            return null;
        }

        return party[index];
    }
}
