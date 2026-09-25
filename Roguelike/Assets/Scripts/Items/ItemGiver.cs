using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable {
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialogue dialogue;

    bool used = false;

    public IEnumerator GiveItem(PlayerController player) {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);

        player.GetComponent<Bag>().AddItem(item, count);

        used = true;

        string dialogueText = $"{player.PlayerName} received {item.ItemName}!";

        if (count > 1) {
            dialogueText = $"{player.PlayerName} received {count} {item.ItemName}s!";
        }

        yield return DialogueManager.Instance.ShowDialogueText(dialogueText);
    }

    public bool CanBeGiven() {
        return item != null && count > 0 && !used;
    }

    public object CaptureState() {
        return used;
    }

    public void RestoreState(object state) {
        used = (bool)state;
    }
}
