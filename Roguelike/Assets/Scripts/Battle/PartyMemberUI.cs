using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HitpointsBar hitpointsBar;
    [SerializeField] Text hitpointsValueText;
    [SerializeField] Color highlightedColour;    

    Pokemon currentPokemon;

    public void SetData(Pokemon pokemon) {
        currentPokemon = pokemon;

        nameText.text = pokemon.Blueprint.GetPokemonName();
        levelText.text = "Lvl: " + pokemon.Level;

        hitpointsValueText.text = "HP: " + pokemon.CurrentHitpoints;

        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.GetHitpoints()));
    }

    public void SetSelected(bool selected) {
        if (selected) {
            nameText.color = highlightedColour;
        } else if (!selected) {
            nameText.color = Color.black;
        }
    }
}
