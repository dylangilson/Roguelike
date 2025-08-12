using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDataBase {
    static Dictionary<string, PokemonBase> pokemon;

    public static void Init() {
        pokemon = new Dictionary<string, PokemonBase>();
        var pokemonBases = Resources.LoadAll<PokemonBase>("");

        foreach (var pokemonBase in pokemonBases) {
            if (pokemon.ContainsKey(pokemonBase.PokemonName)) {
                Debug.LogError($"There are two PokemonBase files with the name {pokemonBase.PokemonName}");

                continue;
            }

            pokemon[pokemonBase.PokemonName] = pokemonBase;
        }
    }

    public static PokemonBase GetPokemonByName(string name) {
        if (!pokemon.ContainsKey(name)) {
            Debug.LogError($"Pokemon with the name {name} not found in PokemonDataBase");

            return null;
        }

        return pokemon[name];
    }
}
