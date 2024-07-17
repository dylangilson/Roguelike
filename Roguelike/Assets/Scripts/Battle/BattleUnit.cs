using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour {
    [SerializeField] PokemonBase blueprint;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    public void Setup() {
        Pokemon = new Pokemon(blueprint, level);

        if (isPlayerUnit) {
            GetComponent<Image>().sprite = Pokemon.Blueprint.GetBackSprite();
        } else {
            GetComponent<Image>().sprite = Pokemon.Blueprint.GetFrontSprite();
        }
    }
}
