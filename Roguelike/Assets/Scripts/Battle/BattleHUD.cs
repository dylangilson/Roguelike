using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HitpointsBar hitpointsBar;
    //[SerializeField] Text hitpointsText;

    Pokemon currentPokemon;

    public void SetData(Pokemon pokemon) {
        currentPokemon = pokemon;
        nameText.text = pokemon.Blueprint.GetPokemonName();
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.GetHitpoints()));
        //hitpointsText.text = "HP: " + pokemon.CurrentHitpoints; // + "/" + pokemon.GetHitpoints();
    }
    
    public IEnumerator UpdateHitpoints() {
        yield return hitpointsBar.SetHitpointsSmooth((float) currentPokemon.CurrentHitpoints / currentPokemon.GetHitpoints());
    }
}
