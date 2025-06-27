using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// this portal teleports the player to a different position without switching scenes

public class LocationPortal : MonoBehaviour, IPlayerTriggerable {
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;
    Fader fader;

    public void OnPlayerTriggered(PlayerController player) {
        this.player = player;

        player.Character.Animator.IsMoving = false;

        StartCoroutine(Teleport());
    }

    private void Start() {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport() {
        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destination = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destination.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }

    public Transform SpawnPoint {
        get {
            return spawnPoint;
        }
    } 
}
