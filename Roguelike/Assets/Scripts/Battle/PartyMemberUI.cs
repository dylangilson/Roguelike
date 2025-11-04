using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HitpointsBar hitpointsBar;
    [SerializeField] Text hitpointsValueText;

    Pokemon currentPokemon;

    public void Init(Pokemon pokemon) {
        currentPokemon = pokemon;

        UpdateData();

        currentPokemon.OnHitpointsChanged += UpdateData;
    }

    public void UpdateData() {
        nameText.text = currentPokemon.Blueprint.PokemonName;
        levelText.text = "Lvl: " + currentPokemon.Level;

        hitpointsValueText.text = "HP: " + currentPokemon.CurrentHitpoints;

        if (currentPokemon.MaxHitpoints != 0){
            hitpointsBar.SetHitpoints((float)(currentPokemon.CurrentHitpoints / currentPokemon.MaxHitpoints));
        }
    }

    public void SetSelected(bool selected) {
        if (selected) {
            nameText.color = GlobalSettings.i.HighlightedColour;
        } else if (!selected) {
            nameText.color = Color.black;
        }
    }
}
