using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item {
    public string name;
    public string description;
    public Sprite sprite;
    public int id;
    public int quantity;

    public Item(string name, string description, Sprite sprite, int id, int quantity) {
        this.name = name;
        this.description = description;
        this.sprite = sprite;
        this.id = id;
        this.quantity = quantity;
    }
}
