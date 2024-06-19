using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public Trainer ash;

    void Start() {
        // Create example items
        Item potion = new Item("Potion", "Restores 20 HP.", null, 1, 1);
        Item pokeball = new Item("Poké Ball", "Catches Pokémon.", null, 2, 5);

        // Create some example moves
        Move tackle = new Move("Tackle", Type.Normal, 40, 95);
        Move ember = new Move("Ember", Type.Fire, 40, 100);
        Move waterGun = new Move("Water Gun", Type.Water, 40, 100);

        // Create a list of moves
        List<Move> charmanderMoves = new List<Move> { tackle, ember };
        List<Move> squirtleMoves = new List<Move> { tackle, waterGun };

        // Create example Pokémon
        Pokemon charmander = new Pokemon("Charmander", Type.Fire, null, 39, charmanderMoves, 5, null, null, null);
        Pokemon squirtle = new Pokemon("Squirtle", Type.Water, null, 44, squirtleMoves, 5, null, null, null);

        // Create a trainer
        Trainer ash = new Trainer("Ash", null);

        // Add items to the trainer's bag
        ash.bag.AddItem(potion);
        ash.bag.AddItem(pokeball);

        // Add Pokémon to the trainer's party
        ash.party.AddPokemon(charmander);
        ash.party.AddPokemon(squirtle);

        // List trainer's inventory and party
        ash.ListInventory();
        ash.ListParty();

        // Example battle interaction
        Pokemon firstPokemon = ash.party.GetPokemon(0);
        Pokemon secondPokemon = ash.party.GetPokemon(1);

        if (firstPokemon != null && secondPokemon != null)
        {
            firstPokemon.UseMove(1, secondPokemon); // Charmander uses Ember on Squirtle
            secondPokemon.UseMove(1, firstPokemon); // Squirtle uses Water Gun on Charmander
        }

        // List party members after the battle interaction
        ash.ListParty();
    }
}
