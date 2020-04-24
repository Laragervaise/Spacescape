using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOnCollision : MonoBehaviour {

    private float threshold = 8;

    void OnCollisionEnter(Collision collision) {
        // if object thrown hard enough on a breakable object, the breakable obkect disappear
        if (collision.relativeVelocity.magnitude >= threshold && collision.gameObject.CompareTag("glass")) {
            Destroy(collision.gameObject);
        }
    }
}
