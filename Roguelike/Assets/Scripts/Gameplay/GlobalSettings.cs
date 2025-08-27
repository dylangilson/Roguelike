using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour {
    [SerializeField] Color highlightedColour;

    public Color HighlightedColour {
        get {
            return highlightedColour;
        }
    }

    public static GlobalSettings i { get; private set; }

    private void Awake() {
        i = this;
    }
}
