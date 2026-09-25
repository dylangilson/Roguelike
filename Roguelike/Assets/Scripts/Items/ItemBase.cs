using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject {
    [SerializeField] string itemName;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] float price;
    [SerializeField] bool isSellable;

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

    public float Price {
        get {
            return price;
        }
    }

    public bool IsSellable {
        get {
            return isSellable;
        }
    }

    public virtual bool CanUseInBattle {
        get {
            return true;
        }
    }

    public virtual bool CanUseOutsideBattle {
        get {
            return true;
        }
    }

    public virtual bool Use(Pokemon pokemon) {
        return false;
    }

    public virtual bool IsReusable {
        get {
            return false;
        }
    }
}
