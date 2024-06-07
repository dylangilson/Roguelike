using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour {
    public List<Item> items = new List<Item>();
    public int maxCapacity = 256;

    public bool AddItem(Item item) {
        if (items.Count >= maxCapacity) {
            Debug.Log("Inventory is full!");
            return false;
        }

        items.Add(item);
        Debug.Log(item.name + " added to inventory.");
        return true;
    }

    public bool RemoveItem(Item item) {
        if (items.Contains(item)) {
            items.Remove(item);
            Debug.Log(item.name + " removed from inventory.");
            return true;
        }

        Debug.Log(item.name + " not found in inventory.");
        return false;
    }

    public void ListItems() {
        Debug.Log("Inventory contains " + items.Count + " items:");
        foreach (var item in items) {
            Debug.Log(item.name + " (x" + item.quantity + ")");
        }
    }
}
