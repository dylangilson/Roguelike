using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable {
    [SerializeField] Pokemon pokemon;
    [SerializeField] Dialogue dialogue;

    bool used = false;

    public IEnumerator GivePokemon(PlayerController player) {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);

        pokemon.Init();

        player.GetComponent<Party>().AddPokemon(pokemon);

        used = true;

        string dialogueText = $"{player.PlayerName} received {pokemon.Blueprint.PokemonName}!";

        dialogueText = $"{player.PlayerName} received {pokemon.Blueprint.PokemonName}!";

        yield return DialogueManager.Instance.ShowDialogueText(dialogueText);
    }

    public bool CanBeGiven() {
        return pokemon != null && !used;
    }

    public object CaptureState() {
        return used;
    }

    public void RestoreState(object state) {
        used = (bool)state;
    }
}
