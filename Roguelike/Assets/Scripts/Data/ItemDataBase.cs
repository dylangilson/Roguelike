using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataBase {
    static Dictionary<string, ItemBase> items;

    public static void Init() {
        items = new Dictionary<string, ItemBase>();
        var itemBases = Resources.LoadAll<ItemBase>("");

        foreach (var itemBase in itemBases) {
            if (items.ContainsKey(itemBase.ItemName)) {
                Debug.LogError($"There are two ItemBase files with the name {itemBase.ItemName}");

                continue;
            }

            items[itemBase.ItemName] = itemBase;
        }
    }

    public static ItemBase GetItemByName(string name) {
        if (!items.ContainsKey(name)) {
            Debug.LogError($"Item with the name {name} not found in ItemDataBase");

            return null;
        }

        return items[name];
    }
}