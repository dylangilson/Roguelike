using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { 
	START, 
	PLAYERTURN, 
	ENEMYTURN, 
	WON, 
	LOST 
}

public class BattleSystem : MonoBehaviour {
	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	public Transform playerBattleStation;
	public Transform enemyBattleStation;
	public Text dialogueText;
	public BattleHUD playerHUD;
	public BattleHUD enemyHUD;
	public BattleState state;
	Pokemon playerPokemon;
	Pokemon enemyPokemon;

    // Start is called before the first frame update
    void Start() {
		state = BattleState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle() {
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
		playerPokemon = playerGO.GetComponent<Pokemon>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		enemyPokemon = enemyGO.GetComponent<Pokemon>();

		dialogueText.text = "A wild " + enemyPokemon.pokemonName + " approaches...";

		playerHUD.SetHUD(playerPokemon);
		enemyHUD.SetHUD(enemyPokemon);

		yield return new WaitForSeconds(2f);

		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

	IEnumerator PlayerAttack() {
		// TODO: update this to account for move used
		bool hasFainted = enemyPokemon.TakeDamage(10);

		enemyHUD.SetHP(enemyPokemon.currentHP);
		dialogueText.text = "The attack is successful!";

		yield return new WaitForSeconds(2f);

		if(hasFainted) {
			state = BattleState.WON;
			EndBattle();
		} else {
			state = BattleState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
		}
	}

	IEnumerator EnemyTurn() {
		dialogueText.text = enemyPokemon.name + " attacks!";

		yield return new WaitForSeconds(1f);

		// TODO: update this to account for move used
		bool hasFainted = playerPokemon.TakeDamage(10);

		playerHUD.SetHP(playerPokemon.currentHP);

		yield return new WaitForSeconds(1f);

		if(hasFainted) {
			state = BattleState.LOST;
			EndBattle();
		} else {
			state = BattleState.PLAYERTURN;
			PlayerTurn();
		}
	}

	void EndBattle() {
		if(state == BattleState.WON) {
			dialogueText.text = "You won the battle!";
		} else if (state == BattleState.LOST) {
			dialogueText.text = "You were defeated.";
		}
	}

	void PlayerTurn() {
		dialogueText.text = "Choose an action:";
	}

	IEnumerator PlayerHeal() {
		playerPokemon.Heal(5);

		playerHUD.SetHP(playerPokemon.currentHP);
		dialogueText.text = "You feel renewed strength!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	public void OnAttackButton() {
		if (state != BattleState.PLAYERTURN) {
			return;
		}

		StartCoroutine(PlayerAttack());
	}

	public void OnHealButton() {
		if (state != BattleState.PLAYERTURN) {
			return;
		}

		StartCoroutine(PlayerHeal());
	}
}
