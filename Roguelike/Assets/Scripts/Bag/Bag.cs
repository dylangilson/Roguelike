using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bag : MonoBehaviour {
    [SerializeField] List<ItemSlot> slots;

    public event Action OnUpdated;

    public List<ItemSlot> Slots {
        get {
            return slots;
        }
    }

    public static Bag GetBag() {
        return FindObjectOfType<PlayerController>().GetComponent<Bag>();
    }

    public ItemBase UseItem(int itemIndex, Pokemon pokemon) {
        var item = slots[itemIndex].Item;

        bool used = item.Use(pokemon);

        if (used) {
            RemoveItem(item);

            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item) {
        var slot = slots.First(slot => slot.Item == item);

        slot.Count--;

        if (slot.Count <= 0) {
            slots.Remove(slot);
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
