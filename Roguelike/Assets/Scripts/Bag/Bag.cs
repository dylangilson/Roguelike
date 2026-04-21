using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { ITEMS, POKEBALLS, TMs }

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
        "ITEMS", "POKÉBALLS", "TMs"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex) {
        return allSlots[categoryIndex];
    }

    public static Bag GetBag() {
        return FindObjectOfType<PlayerController>().GetComponent<Bag>();
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex) {
        var currentSlots = GetSlotsByCategory(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon pokemon, int selectedCategory) {
        var item = GetItem(itemIndex, selectedCategory);

        bool used = item.Use(pokemon);

        if (used) {
            if (!item.IsReusable) {
                RemoveItem(item, selectedCategory);
            }
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count=1) {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        
        if (itemSlot != null) {
            itemSlot.Count += count;
        } else {
            currentSlots.Add(new ItemSlot() {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
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
    
    ItemCategory GetCategoryFromItem(ItemBase item) {
        if (item is RecoveryItem) {
            return ItemCategory.ITEMS;
        } else if (item is Pokeball) {
            return ItemCategory.POKEBALLS;
        } else {
            return ItemCategory.TMs;
        } 
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
        
        set {
            item = value;
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
