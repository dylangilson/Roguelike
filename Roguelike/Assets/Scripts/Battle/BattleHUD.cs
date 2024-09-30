using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HitpointsBar hitpointsBar;
    [SerializeField] Text hitpointsValueText;

    Pokemon currentPokemon;

    public void SetData(Pokemon pokemon) {
        currentPokemon = pokemon;
        nameText.text = pokemon.Blueprint.GetPokemonName();
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.GetHitpoints()));
        hitpointsValueText.text = "HP: " + pokemon.CurrentHitpoints;// + "/" + pokemon.GetHitpoints().ToString();
    }
    
    public IEnumerator UpdateHitpoints() {
        yield return hitpointsBar.SetHitpointsSmooth((float) currentPokemon.CurrentHitpoints / currentPokemon.GetHitpoints());
    }
    
    public IEnumerator UpdateHitpointsValue() {
        yield return hitpointsValueText.text = "HP: " + currentPokemon.CurrentHitpoints;
    }
}
