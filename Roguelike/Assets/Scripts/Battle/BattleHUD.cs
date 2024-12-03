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

        nameText.text = pokemon.Blueprint.PokemonName;
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsValueText.text = "HP: " + pokemon.CurrentHitpoints;
        
        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.MaxHitpoints));
    }
    
    public IEnumerator UpdatePlayerHitpoints() {
        hitpointsValueText.text = "HP: " + currentPokemon.CurrentHitpoints;
        if (currentPokemon.HitpointsChanged) {
            yield return hitpointsBar.SetHitpointsSmooth((float) currentPokemon.CurrentHitpoints / currentPokemon.MaxHitpoints);
        }
    }

    public IEnumerator UpdateEnemyHitpoints() {
        if (currentPokemon.HitpointsChanged) {
            yield return hitpointsBar.SetHitpointsSmooth((float) currentPokemon.CurrentHitpoints / currentPokemon.MaxHitpoints);
        }
    }
}
