using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create New Pokeball Item")]
public class Pokeball : ItemBase {
    public override bool Use(Pokemon pokemon) {
        return true;
    }
}
