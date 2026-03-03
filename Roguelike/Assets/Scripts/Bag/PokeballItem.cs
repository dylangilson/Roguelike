using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create New Pokeball Item")]
public class Pokeball : ItemBase {
    [SerializeField] float catchRateModifier = 1;

    public override bool Use(Pokemon pokemon) {
        if (GameController.Instance.State == GameState.BATTLE){
            return true;
        }
        return false;
    }

    public float CatchRateModifier => catchRateModifier;
}
