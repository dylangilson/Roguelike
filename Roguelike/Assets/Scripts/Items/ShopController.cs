using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum ShopState { MENU, BUYING, SELLING, BUSY }

public class ShopController : MonoBehaviour {
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public event Action OnStart;
    public event Action OnFinish;

    public static ShopController i { get; private set; }
    
    ShopState state;
    Bag bag;
    Shopkeeper shopkeeper;

    private void Awake() {
        i = this;
    }

    private void Start() {
        bag = Bag.GetBag();
    }

    public IEnumerator StartTrading(Shopkeeper shopkeeper) {
        this.shopkeeper = shopkeeper;
        OnStart?.Invoke();

        yield return StartMenuState();
    }

    IEnumerator StartMenuState() {
        state = ShopState.MENU;

        int selectedChoice = -1;

        yield return DialogueManager.Instance.ShowDialogueText("How may I help you?", waitForInput: false, 
            choices: new List<string>() { "Buy", "Sell", "Quit" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex,
            onCancel: () => OnFinish?.Invoke());

        if (selectedChoice == 0) {
            // buy
            state = ShopState.BUYING;
            walletUI.Show();
            shopUI.Show(shopkeeper.AvailableItems, (item) => StartCoroutine(BuyItem(item)), OnBackFromBuying);
        } else if (selectedChoice == 1) {
            // sell
            state = ShopState.SELLING;
            walletUI.Show();

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
        } else if (state == ShopState.BUYING) {
            shopUI.HandleUpdate();
        }
    }

    void OnBackFromSelling() {
        inventoryUI.gameObject.SetActive(false);
        walletUI.Close();

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
        int countToSell = 1;
        int itemCount = bag.GetItemCount(item);

        bool cancelled = false;

        if (itemCount > 1) {
            yield return DialogueManager.Instance.ShowDialogueText($"How many would you like to sell?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice, (selectedCount) => countToSell = selectedCount,
                onCancel: () => cancelled = true);

            DialogueManager.Instance.CloseDialogue();

            if (cancelled) {
                state = ShopState.SELLING;
                yield break;
            }
        }

        sellingPrice *= countToSell;

        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText($"I can give ¥{sellingPrice} for that! Would you like to sell?", waitForInput: false, 
            choices: new List<string>() { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0) {
            // yes
            bag.RemoveItem(item, countToSell);

            Wallet.i.AddMoney(sellingPrice);

            yield return DialogueManager.Instance.ShowDialogueText($"Turned over {item.ItemName} and received ¥{sellingPrice}!");
        }

        state = ShopState.SELLING;
    }

    private void CancelSale() {
        OnFinish?.Invoke();
        DialogueManager.Instance.CloseDialogue();
    }

    IEnumerator BuyItem(ItemBase item) {
        state = ShopState.BUSY;

        yield return DialogueManager.Instance.ShowDialogueText("How many would you like to buy?", waitForInput: false, autoClose: false);

        int countToBuy = 1;

        yield return countSelectorUI.ShowSelector(100, item.Price, (selectedCount) => countToBuy = selectedCount);

        DialogueManager.Instance.CloseDialogue();

        float totalPrice = item.Price * countToBuy;

        if (Wallet.i.HasEnoughMoney(totalPrice)) {
            int selectedChoice = 0;

            yield return DialogueManager.Instance.ShowDialogueText($"That will be ¥{totalPrice}!", waitForInput: false, 
                choices: new List<string>() { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0) {
                // yes
                bag.AddItem(item, countToBuy);

                Wallet.i.TakeMoney(totalPrice);

                yield return DialogueManager.Instance.ShowDialogueText("Thank you for shopping with us!");
            }
        } else {
            yield return DialogueManager.Instance.ShowDialogueText("You don't have enough money!");
        }

        state = ShopState.BUYING;
    }

    private void OnBackFromBuying() {
        shopUI.Close();
        walletUI.Close();

        StartCoroutine(StartMenuState());
    }
}
