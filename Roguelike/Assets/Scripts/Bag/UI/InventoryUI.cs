using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState {ITEM_SELECTION, PARTY_SELECTION, BUSY, MOVE_TO_FORGET}

public class InventoryUI : MonoBehaviour {
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Text itemCategory;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    Action<ItemBase> onItemUsed;

    int selectedItem = 0;
    int selectedCategory = 0;
    MoveBase moveToLearn;
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

        foreach (var itemSlot in bag.GetSlotsByCategory(selectedCategory)) {
            var slotUIObject = Instantiate(itemSlotUI, itemList.transform);

            slotUIObject.SetData(itemSlot);

            slotUIlist.Add(slotUIObject);
        }

        scrollOffset = 0;

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null) {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ITEM_SELECTION) {
            int previousSelectedItem = selectedItem;
            int previousSelectedCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
                selectedItem++;
            } else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
                selectedItem--;
            } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                selectedCategory++;
            } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
                selectedCategory--;
            }

            selectedCategory = (selectedCategory + Bag.ItemCategories.Count) % Bag.ItemCategories.Count;
            if (bag.GetSlotsByCategory(selectedCategory).Count > 0){
                selectedItem = (selectedItem + bag.GetSlotsByCategory(selectedCategory).Count) % bag.GetSlotsByCategory(selectedCategory).Count;
            }
            
            if (previousSelectedCategory != selectedCategory) {
                ResetSelection();
                itemCategory.text = Bag.ItemCategories[selectedCategory];
                UpdateItemList();
            } else if (previousSelectedItem != selectedItem) {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) {
                StartCoroutine(ItemSelected());
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
        } else if (state == InventoryUIState.MOVE_TO_FORGET) {
            Action<int> onMoveSelected = (moveIndex) => {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected() {
        state = InventoryUIState.BUSY;
        var item = bag.GetItem(selectedItem, selectedCategory);
        
        if (GameController.Instance.State == GameState.BATTLE) {
            // in battle
            if (!item.CanUseInBattle) {
                yield return DialogueManager.Instance.ShowDialogueText($"{item.ItemName} cannot be used in battle!");      
                state = InventoryUIState.ITEM_SELECTION;
                yield break;          
            }
        } else {
            // outside battle
            if (!item.CanUseOutsideBattle) {
                yield return DialogueManager.Instance.ShowDialogueText($"{item.ItemName} cannot be used outside of a battle!");      
                state = InventoryUIState.ITEM_SELECTION;
                yield break;          
            }
        }
        if (selectedCategory == (int)ItemCategory.POKEBALLS) {
            StartCoroutine(UseItem());
        } else {
            OpenPartyScreen();

            if (item is TMItem) {
                partyScreen.ShowIfTMIsUsable(item as TMItem);                
            }
        }
    }

    private IEnumerator UseItem() {
        state = InventoryUIState.BUSY;

        yield return HandleTMItems();

        var item = bag.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (item != null) {
            if (item is RecoveryItem) {
                yield return DialogueManager.Instance.ShowDialogueText($"Player used {item.ItemName} on {partyScreen.SelectedMember.Blueprint.PokemonName}!");
            }

            onItemUsed?.Invoke(item);
        } else {
            if (selectedCategory == (int)ItemCategory.ITEMS) {
                yield return DialogueManager.Instance.ShowDialogueText($"It won't have any effect on {partyScreen.SelectedMember.Blueprint.PokemonName}!");
            }
        }

        ClosePartyScreen();
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove) {
        state = InventoryUIState.BUSY;

        yield return DialogueManager.Instance.ShowDialogueText("Choose a move you want to forget!");

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Blueprint).ToList(), newMove);

        moveToLearn = newMove;

        state = InventoryUIState.MOVE_TO_FORGET;
    }

    IEnumerator HandleTMItems() {
        var item = bag.GetItem(selectedItem, selectedCategory) as TMItem;

        if (item == null) {
            yield break;
        }

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(item.Move)) {
            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} already knows {item.Move.MoveName}!");
            yield break;
        }

        if (!(item.CanBeTaught(pokemon))) {
            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} cannot learn {item.Move.MoveName}!");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MAX_NUMBER_OF_MOVES) {
            pokemon.LearnMove(item.Move);

            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} learned {item.Move.MoveName}!");
        } else {
            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} is trying to learn {item.Move.MoveName}!");
            yield return DialogueManager.Instance.ShowDialogueText($"But it cannot learn more than {PokemonBase.MAX_NUMBER_OF_MOVES} moves!");

            yield return ChooseMoveToForget(pokemon, item.Move);

            yield return new WaitUntil(() => state != InventoryUIState.MOVE_TO_FORGET);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void UpdateItemSelection() {
        var slots = bag.GetSlotsByCategory(selectedCategory);
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIlist.Count; i++) {
            if (i == selectedItem) {
                slotUIlist[i].NameText.color = GlobalSettings.i.HighlightedColour;
            } else {
                slotUIlist[i].NameText.color = Color.black;
            }
        }

        if (slots.Count > 0) {
            var item = slots[selectedItem].Item;

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

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

    void ResetSelection() {
        selectedItem = 0;
        
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen() {
        state = InventoryUIState.PARTY_SELECTION;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen() {
        state = InventoryUIState.ITEM_SELECTION;
        partyScreen.ClearMessage();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex) {
        var pokemon = partyScreen.SelectedMember;

        moveSelectionUI.gameObject.SetActive(false);

        if (moveIndex == PokemonBase.MAX_NUMBER_OF_MOVES) {
            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} did not learn {moveToLearn.MoveName}!");

            moveToLearn = null;
            ClosePartyScreen();
        } else {
            var selectedMove = pokemon.Moves[moveIndex].Blueprint;

            yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} forgot {selectedMove.MoveName} and learned {moveToLearn.MoveName}!");
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
    
            state = InventoryUIState.ITEM_SELECTION;
        }
    }
}
