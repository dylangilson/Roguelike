using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bag : MonoBehaviour {
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> technicalMachineSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake() {
        allSlots = new List<List<ItemSlot>>() { slots, pokeballSlots, technicalMachineSlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>() {
        "ITEMS", "POKEBALLS", "TMs & HMs"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex) {
        return allSlots[categoryIndex];
    }

    public static Bag GetBag() {
        return FindObjectOfType<PlayerController>().GetComponent<Bag>();
    }

    public ItemBase UseItem(int itemIndex, Pokemon pokemon, int selectedCategory) {
        var currentSlots = GetSlotsByCategory(selectedCategory);

        var item = currentSlots[itemIndex].Item;

        bool used = item.Use(pokemon);

        if (used) {
            RemoveItem(item, selectedCategory);

            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item, int selectedCategory) {
        var currentSlots = GetSlotsByCategory(selectedCategory);

        var slot = currentSlots.First(slot => slot.Item == item);

        slot.Count--;

        if (slot.Count <= 0) {
            currentSlots.Remove(slot);
        }

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot {
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item {
        get {
            return item;
        }
    }

    public int Count {
        get {
            return count;
        }

        set {
            count = value;
        }
    }
}
