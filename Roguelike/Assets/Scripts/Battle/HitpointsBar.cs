using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitpointsBar : MonoBehaviour {
    [SerializeField] GameObject hitpoints;
    
    public void SetHitpoints(float normalizedHitpoints) {
        hitpoints.transform.localScale = new Vector3(normalizedHitpoints, 1.0f);
    }

    public IEnumerator SetHitpointsSmooth(float newHitpoints) {
        float currentHitpoints = hitpoints.transform.localScale.x;
        float changeAmount = currentHitpoints - newHitpoints;

        while (currentHitpoints - newHitpoints > Mathf.Epsilon) {
            currentHitpoints -= changeAmount * Time.deltaTime; 
            hitpoints.transform.localScale = new Vector3(currentHitpoints, 1.0f);

            yield return currentHitpoints;
        }
        
        hitpoints.transform.localScale = new Vector3(newHitpoints, 1.0f);
    }
}
