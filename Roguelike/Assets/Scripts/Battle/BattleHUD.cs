using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HitpointsBar hitpointsBar;

    public void SetData(Pokemon pokemon) {
        nameText.text = pokemon.Blueprint.GetPokemonName();
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.GetMaxHitpoints()));
    }
}
