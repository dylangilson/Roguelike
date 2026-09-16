using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour {
    public IEnumerator Heal(Transform player, Dialogue dialogue) {
        int selectedChoice = 0;
        yield return DialogueManager.Instance.ShowDialogue(dialogue, new List<string>() { "Yes", "No" }, (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0) {
            yield return Fader.i.FadeIn(0.5f);

            var party = player.GetComponent<Party>();

            party.GetParty().ForEach(pokemon => pokemon.Heal());
            party.PartyUpdated();

            yield return Fader.i.FadeOut(0.5f);
            yield return DialogueManager.Instance.ShowDialogueText($"All done! Your Pokemon are fully healed now!");
        
        } else if (selectedChoice == 1) {
            yield return DialogueManager.Instance.ShowDialogueText($"Okay! Come back if you need a rest!");
        }

        
    }
}
