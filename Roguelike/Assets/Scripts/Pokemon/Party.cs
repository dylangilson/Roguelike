using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Party : MonoBehaviour {
    [SerializeField] List<Pokemon> party;


    public List<Pokemon> GetParty() {
        return party;
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
}
