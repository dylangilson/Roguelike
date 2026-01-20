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
        if (currentPokemon != null) {
            currentPokemon.OnStatusChanged -= SetStatusText;
            currentPokemon.OnHitpointsChanged -= UpdateHitpoints;
        }

        currentPokemon = pokemon;

        nameText.text = pokemon.Blueprint.PokemonName;
        levelText.text = "Lvl: " + pokemon.Level;
        hitpointsValueText.text = $"{currentPokemon.CurrentHitpoints} / {currentPokemon.MaxHitpoints}";
        
        SetLevel();
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
        currentPokemon.OnHitpointsChanged += UpdateHitpoints;
    }

    void SetStatusText() {
        if (currentPokemon.Status == null) {
            statusText.text = "";
        } else if (currentPokemon.Status != null) {
            statusText.text = currentPokemon.Status.Abbreviation.ToString().ToUpper();
            statusText.color = statusColors[currentPokemon.Status.ID];
        }
    }

    public void SetLevel() {
        levelText.text = "Lvl: " + currentPokemon.Level;
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
    public IEnumerator SetExpSmooth(bool reset) {
        if (expBar == null) {
            yield break;
        }

        if (reset) {
            expBar.transform.localScale = new Vector3(0.0f, 1.0f, 1.0f);
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
    public void UpdateHitpoints() {
        StartCoroutine(UpdateHitpointsAsync());
    }

    public IEnumerator UpdateHitpointsAsync() {
        hitpointsValueText.text = $"{currentPokemon.CurrentHitpoints} / {currentPokemon.MaxHitpoints}";
        yield return hitpointsBar.SetHitpointsSmooth((float) currentPokemon.CurrentHitpoints / currentPokemon.MaxHitpoints);
    }

    public IEnumerator WaitForHitpointsUpdate() {
        yield return new WaitUntil(() => hitpointsBar.IsUpdating == false);
    }

    public void ClearData() {
        if (currentPokemon != null) {
            currentPokemon.OnStatusChanged -= SetStatusText;
            currentPokemon.OnHitpointsChanged -= UpdateHitpoints;
        }
    }
}
