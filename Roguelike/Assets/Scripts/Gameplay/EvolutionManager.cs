using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour {
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private void Awake() {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution) {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        pokemonImage.sprite = pokemon.Blueprint.FrontSprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{pokemon.Blueprint.PokemonName} is evolving!");

        var oldPokemon = pokemon.Blueprint;
        pokemon.Evolve(evolution);

        pokemonImage.sprite = pokemon.Blueprint.FrontSprite;
        yield return DialogueManager.Instance.ShowDialogueText($"{oldPokemon.PokemonName} evolved into {pokemon.Blueprint.PokemonName}!");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
