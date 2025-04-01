using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour {
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;

    public static GameLayers Instance { get; set; }

    private void Awake() {
        Instance = this;
    }

    public LayerMask SolidObjectsLayer {
        get {
            return solidObjectsLayer;
        }
    }

    public LayerMask InteractableLayer {
        get {
            return interactableLayer;
        }
    }

    public LayerMask GrassLayer {
        get {
            return grassLayer;
        }
    }

    public LayerMask PlayerLayer {
        get {
            return playerLayer;
        }
    }

    public LayerMask FOVLayer {
        get {
            return fovLayer;
        }
    }
}
