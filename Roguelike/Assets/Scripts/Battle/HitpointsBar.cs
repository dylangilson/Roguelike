using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitpointsBar : MonoBehaviour {
    [SerializeField] GameObject hitpoints;
    
    public void SetHitpoints(float normalizedHitpoints) {
        hitpoints.transform.localScale = new Vector3(normalizedHitpoints, 1.0f);
    }
}
