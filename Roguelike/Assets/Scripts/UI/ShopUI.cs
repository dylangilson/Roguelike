using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour {
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedItem;

    List<ItemBase> availableItems;
    List<ItemSlotUI> slotUIlist;
    
    Action<ItemBase> onItemSelected;
    Action onBack;
    
    RectTransform itemListTransform;
    
    const int ITEMS_IN_VIEWPORT = 7;

    private void Awake() {
        itemListTransform = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack) {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;
        
        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void HandleUpdate() {
        var previousSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            selectedItem--;
        } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            selectedItem++;
        }
        
        selectedItem = selectedItem % availableItems.Count;

        if (selectedItem != previousSelection) {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
            onItemSelected?.Invoke(availableItems[selectedItem]);
        } else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape)) {
            onBack?.Invoke();
        }
    }

    private void UpdateItemList() {
        // clear all existing items
        foreach (Transform child in itemList.transform) {
            Destroy(child.gameObject);
        }

        // load items
        slotUIlist = new List<ItemSlotUI>();

        foreach (var item in availableItems) {
            var slotUIObject = Instantiate(itemSlotUI, itemList.transform);

            slotUIObject.SetNameAndPrice(item);

            slotUIlist.Add(slotUIObject);
        }

        UpdateItemSelection();
    }

    private void UpdateItemSelection() {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        for (int i = 0; i < slotUIlist.Count; i++) {
            if (i == selectedItem) {
                slotUIlist[i].NameText.color = GlobalSettings.i.HighlightedColour;
            } else {
                slotUIlist[i].NameText.color = Color.black;
            }
        }

        if (availableItems.Count > 0) {
            var item = availableItems[selectedItem];

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    private void HandleScrolling() {
        if (slotUIlist.Count == ITEMS_IN_VIEWPORT) {
            return;
        }

        float scrollPosition = Mathf.Clamp(selectedItem - ITEMS_IN_VIEWPORT / 2, 0, selectedItem) * slotUIlist[0].Height;
        
        itemListTransform.localPosition = new Vector2(itemListTransform.localPosition.x, scrollPosition);

        // update arrows visibility
        bool showUpArrow = selectedItem > ITEMS_IN_VIEWPORT / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + ITEMS_IN_VIEWPORT / 2 < slotUIlist.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
