using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create New TM")]
public class TMItem : ItemBase {
    [SerializeField] MoveBase move;

    public MoveBase Move => move;

    public override bool CanUseInBattle => false;

    public override bool IsReusable => true;

    public override bool Use(Pokemon pokemon) {
        // learning move is handled in InventoryUI
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon) {
        return pokemon.Blueprint.TMMoves.Contains(move);
    }
}
