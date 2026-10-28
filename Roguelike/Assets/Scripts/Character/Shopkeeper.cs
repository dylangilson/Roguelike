using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopkeeper : MonoBehaviour {
    [SerializeField] List<ItemBase> availableItems;

    public IEnumerator Trade() {
        yield return ShopController.i.StartTrading(this);
    }

    public List<ItemBase> AvailableItems {
        get {
            return availableItems;
        }
    }
}
