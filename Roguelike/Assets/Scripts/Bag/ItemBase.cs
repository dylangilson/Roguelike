using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject {
    [SerializeField] string itemName;
    [SerializeField] string description;
    [SerializeField] Sprite icon;

    public string ItemName {
        get {
            return itemName;
        }
    }

    public string Description {
        get {
            return description;
        }
    }

    public Sprite Icon {
        get {
            return icon;
        }
    }
}
