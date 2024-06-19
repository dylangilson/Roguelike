using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour {
	public Text nameText;
	public Text levelText;
	public Slider hpSlider;

	public void SetHUD(Pokemon pokemon) {
		nameText.text = pokemon.pokemonName;
		levelText.text = "Lvl " + pokemon.level;
		hpSlider.maxValue = pokemon.maxHP;
		hpSlider.value = pokemon.currentHP;
	}

	public void SetHP(int hp) {
		hpSlider.value = hp;
	}
}
