using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameObjects : MonoBehaviour {
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
}
