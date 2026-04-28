using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable {
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator) {
        if (!Used) {
            initiator.GetComponent<Bag>().AddItem(item);
        
            Used = true;
            
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            string playerName = initiator.GetComponent<PlayerController>().PlayerName;
        
            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} found {item.ItemName}!");
        }
    }
}
