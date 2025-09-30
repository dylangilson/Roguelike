using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour {
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    int selectedItem = 0;
    List<ItemSlotUI> slotUIlist;
    Bag bag;

    private void Awake() {
        bag = Bag.GetBag();
    }

    private void Start() {
        UpdateItemList();
    }

    void UpdateItemList() {
        // clear all existing items
        foreach (Transform child in itemList.transform) {
            Destroy(child.gameObject);
        }

        // load items
        slotUIlist = new List<ItemSlotUI>();

        foreach (var itemSlot in bag.Slots) {
            var slotUIObject = Instantiate(itemSlotUI, itemList.transform);

            slotUIObject.SetData(itemSlot);

            slotUIlist.Add(slotUIObject);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack) {
        int previousSelectedItem = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            selectedItem++;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            selectedItem--;
        }

        selectedItem = (selectedItem + bag.Slots.Count) % bag.Slots.Count;

        if (previousSelectedItem != selectedItem) {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            onBack?.Invoke();
        }
    }

    public void UpdateItemSelection() {
        for (int i = 0; i < slotUIlist.Count; i++) {
            if (i == selectedItem) {
                slotUIlist[i].NameText.color = GlobalSettings.i.HighlightedColour;
            } else {
                slotUIlist[i].NameText.color = Color.black;
            }
        }

        var item = bag.Slots[selectedItem].Item;

        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;
    }
}
