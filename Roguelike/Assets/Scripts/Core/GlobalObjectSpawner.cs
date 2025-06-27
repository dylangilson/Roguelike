using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjectSpawner : MonoBehaviour {
    [SerializeField] GameObject GlobalGameObjectsPrefab;

    private void Awake() {
        var existingObjects = FindObjectsOfType<GlobalGameObjects>();
        
        if (existingObjects.Length == 0) {
            var spawnPosition = new Vector3(0.0f, 0.0f, 0.0f); // set this position to a unique position scene to scene by editing the Player Position in the inspector
            var grid = FindObjectOfType<Grid>();

            if (grid != null) {
                spawnPosition = grid.transform.position;
            }

            Instantiate(GlobalGameObjectsPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
