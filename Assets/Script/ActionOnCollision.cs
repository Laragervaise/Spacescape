using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOnCollision : MonoBehaviour {

    private float threshold = 8;

    void OnCollisionEnter(Collision collision) {
        // if object thrown hard enough on a tagged object, this tagged object disappears
        if (collision.relativeVelocity.magnitude >= threshold && collision.gameObject.CompareTag("glass")) {
            Destroy(collision.gameObject); // or trigger whatever action we need (like opening a door by destroying another gameObject)
        }
    }
}
