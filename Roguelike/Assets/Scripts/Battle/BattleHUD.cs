using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HitpointsBar hitpointsBar;
    [SerializeField] Text hitpointsValueText;

    [SerializeField] Color poisonColor;
    [SerializeField] Color paralysisColor;
    [SerializeField] Color sleepColor;
    [SerializeField] Color burnColor;
    [SerializeField] Color freezeColor;

    Pokemon currentPokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon) {
        currentPokemon = pokemon;

        nameText.text = pokemon.Blueprint.PokemonName;
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsValueText.text = "HP: " + pokemon.CurrentHitpoints;
        
        hitpointsBar.SetHitpoints((float)(pokemon.CurrentHitpoints / pokemon.MaxHitpoints));

        statusColors = new Dictionary<ConditionID, Color>() {
            {ConditionID.POISON, poisonColor},
            {ConditionID.BURN, burnColor},
            {ConditionID.SLEEP, sleepColor},
            {ConditionID.PARALYSIS, paralysisColor},
            {ConditionID.FREEZE, freezeColor},
        };

        SetStatusText();
        currentPokemon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText() {
        if (currentPokemon.Status == null) {
            statusText.text = "";
        } else if (currentPokemon.Status != null) {
            statusText.text = currentPokemon.Status.Abbreviation.ToString().ToUpper();
            statusText.color = statusColors[currentPokemon.Status.ID];
        }
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
