using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState {ITEM_SELECTION, PARTY_SELECTION, BUSY}

public class InventoryUI : MonoBehaviour {
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;

    Action onItemUsed;

    int selectedItem = 0;
    int scrollOffset = 0;
    InventoryUIState state;
    List<ItemSlotUI> slotUIlist;
    Bag bag;
    RectTransform itemListTransform;
    
    const int ITEMS_IN_VIEWPORT = 7;

    private void Awake() {
        bag = Bag.GetBag();
        itemListTransform = itemList.GetComponent<RectTransform>();
    }

    private void Start() {
        UpdateItemList();

        bag.OnUpdated += UpdateItemList;
    }

    private void UpdateItemList() {
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

        scrollOffset = 0;

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action onItemUsed = null) {
        this.onItemUsed = onItemUsed;
        if (state == InventoryUIState.ITEM_SELECTION) {
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

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
                OpenPartyScreen();
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                onBack?.Invoke();
            }
        } else if (state == InventoryUIState.PARTY_SELECTION) {
            Action onSelected = () => {
                StartCoroutine(UseItem());
            };

            Action onClosePartyScreen = () => {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onClosePartyScreen);
        } 
    }

    private IEnumerator UseItem() {
        state = InventoryUIState.BUSY;

        var item = bag.UseItem(selectedItem, partyScreen.SelectedMember);

        if (item != null) {
            yield return DialogueManager.Instance.ShowDialogueText($"Player used {item.ItemName} on {partyScreen.SelectedMember.Blueprint.PokemonName}!");
            onItemUsed?.Invoke();
        } else {
            yield return DialogueManager.Instance.ShowDialogueText($"It won't have any effect on {partyScreen.SelectedMember.Blueprint.PokemonName}!");
        }

        ClosePartyScreen();
    }

    private void UpdateItemSelection() {
        selectedItem = Mathf.Clamp(selectedItem, 0, bag.Slots.Count - 1);

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

        HandleScrolling();
    }

    private void HandleScrolling() {
        if (slotUIlist.Count == 0) {
            return;
        }

        // if the selection moved beyond the visible area, adjust scroll offset
        if (selectedItem < scrollOffset) {
            scrollOffset = selectedItem;
        } else if (selectedItem >= scrollOffset + ITEMS_IN_VIEWPORT) {
            scrollOffset = selectedItem - ITEMS_IN_VIEWPORT + 1;
        }

        float scrollPosition = scrollOffset * slotUIlist[0].Height;
        
        itemListTransform.localPosition = new Vector2(itemListTransform.localPosition.x, scrollPosition);

        // update arrows visibility
        bool showUpArrow = scrollOffset > 0;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = scrollOffset + ITEMS_IN_VIEWPORT < slotUIlist.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void OpenPartyScreen() {
        state = InventoryUIState.PARTY_SELECTION;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen() {
        state = InventoryUIState.ITEM_SELECTION;
        partyScreen.gameObject.SetActive(false);
    }
}
