using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjectSpawner : MonoBehaviour {
    [SerializeField] GameObject GlobalGameObjectsPrefab;

    private void Awake() {
        var existingObjects = FindObjectsOfType<GlobalGameObjects>();
        
        if (existingObjects.Length == 0) {
            Instantiate(GlobalGameObjectsPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}
