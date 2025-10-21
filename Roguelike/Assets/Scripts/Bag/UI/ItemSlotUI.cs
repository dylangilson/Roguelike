using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text countText;

    RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public Text NameText {
        get {
            return nameText;
        }
    }

    public Text CountText {
        get {
            return countText;
        }
    }

    public float Height {
        get {
            return rectTransform.rect.height;
        }
    }

    public void SetData(ItemSlot itemSlot) {
        nameText.text = itemSlot.Item.ItemName;
        countText.text = $"X {itemSlot.Count}";
    }
}
