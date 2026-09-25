using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ShopState { MENU, BUYING, SELLING, BUSY }

public class ShopController : MonoBehaviour {
    [SerializeField] InventoryUI inventoryUI;

    public event Action OnStart;
    public event Action OnFinish;

    public static ShopController i { get; private set; }
    
    ShopState state;
    Bag bag;

    private void Awake() {
        i = this;
    }

    private void Start() {
        bag = Bag.GetBag();
    }

    public IEnumerator StartTrading(Shopkeeper shopkeeper) {
        OnStart?.Invoke();

        yield return StartMenuState();
    }

    IEnumerator StartMenuState() {
        state = ShopState.MENU;

        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText("How may I help you?", waitForInput: false, 
        choices: new List<string>() { "Buy", "Sell", "Quit" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0) {
            // buy
        } else if (selectedChoice == 1) {
            // sell
            state = ShopState.SELLING;

            inventoryUI.gameObject.SetActive(true);
        } else if (selectedChoice == 2) {
            // quit
            OnFinish?.Invoke();

            yield break;
        }
    }

    public void HandleUpdate() {
        if (state == ShopState.SELLING) {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
    }

    void OnBackFromSelling() {
        inventoryUI.gameObject.SetActive(false);

        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item) {
        state = ShopState.BUSY;

        if (!item.IsSellable) {
            yield return DialogueManager.Instance.ShowDialogueText("You can't sell that!");

            state = ShopState.SELLING;

            yield break;
        }

        float sellingPrice = Mathf.Round(item.Price * 0.6f); // 60% of actual price
        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText($"I can give {sellingPrice} for that! Would you like to sell?", waitForInput: false, 
        choices: new List<string>() { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0) {
            // yes
            bag.RemoveItem(item);

            // TODO: add selling price of item to player's balance

            yield return DialogueManager.Instance.ShowDialogueText($"Turned over {item.ItemName} and received {sellingPrice}!");
        }

        state = ShopState.SELLING;
    }
}
