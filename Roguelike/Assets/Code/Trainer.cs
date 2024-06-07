using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trainer {
    public string name;
    public Bag bag;
    public Party party;
    public Sprite sprite;

    public Trainer(string name, Sprite sprite) {
        this.name = name;
        this.bag = new Bag();
        this.party = new Party();
        this.sprite = sprite;
    }

    public void ListInventory() {
        Debug.Log(name + "'s Inventory:");
        bag.ListItems();
    }

    public void ListParty() {
        Debug.Log(name + "'s Party:");
        party.ListParty();
    }
}
