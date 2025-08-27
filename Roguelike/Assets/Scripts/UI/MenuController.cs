using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {
    [SerializeField] GameObject menu;
    [SerializeField] Color highlightedColour;

    public event Action<int> onMenuItemSelected;
    public event Action onBack;

    List<Text> menuItems;
    int selectedItem = 0;

    private void Awake() {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu() {
        menu.SetActive(true);

        selectedItem = 0;

        UpdateItemSelection();
    }

    public void CloseMenu() {
        menu.SetActive(false);
    }

    public void HandleUpdate() {
        int previousSelectedItem = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            selectedItem++;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            selectedItem--;
        }

        selectedItem = (selectedItem + menuItems.Count) % menuItems.Count;

        if (previousSelectedItem != selectedItem) {
            UpdateItemSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            onBack?.Invoke();

            CloseMenu();
        } else if (Input.GetKeyDown(KeyCode.Return)) {
            onMenuItemSelected?.Invoke(selectedItem);

            CloseMenu();
        }
    }

    public void UpdateItemSelection() {
        for (int i = 0; i < menuItems.Count; i++) {
            if (i == selectedItem) {
                menuItems[i].color = GlobalSettings.i.HighlightedColour;
            } else {
                menuItems[i].color = Color.black;
            }
        }
    }
}
