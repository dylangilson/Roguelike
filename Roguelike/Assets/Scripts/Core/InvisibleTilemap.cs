using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InvisibleTilemap : MonoBehaviour {
    void Start() {
       GetComponent<TilemapRenderer>().enabled = false; 
    }
}
