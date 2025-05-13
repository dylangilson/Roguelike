using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHUD : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HitpointsBar hitpointsBar;
    [SerializeField] Text hitpointsValueText;
    [SerializeField] GameObject expBar;

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
        SetExp();

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

    // use this to initialize HUD
    public void SetExp() {
        if (expBar == null) {
            return;
        }

        float normalizedExp = GetNormalizedExp();

        expBar.transform.localScale = new Vector3(normalizedExp, 1.0f, 1.0f);
    }

    // use this when gaining exp
    public IEnumerator SetExpSmooth() {
        if (expBar == null) {
            yield break;
        }

        float normalizedExp = GetNormalizedExp();

        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    public float GetNormalizedExp() {
        int currentLevelExp = currentPokemon.Blueprint.GetExp(currentPokemon.Level);
        int nextLevelExp = currentPokemon.Blueprint.GetExp(currentPokemon.Level + 1);

        float normalizedExp = (float)(currentPokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);

        return Mathf.Clamp01(normalizedExp);
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
