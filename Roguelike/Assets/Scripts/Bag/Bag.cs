using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour {
    [SerializeField] List<ItemSlot> slots;

    public List<ItemSlot> Slots {
        get {
            return slots;
        }
    }

    public static Bag GetBag() {
        return FindObjectOfType<PlayerController>().GetComponent<Bag>();
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
    }
}
