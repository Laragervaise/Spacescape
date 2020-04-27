using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour {

    private float threshold = 8;

    void OnCollisionEnter(Collision collision) {
        // if object thrown hard enough on this, trigger the opening of the door
        if (collision.relativeVelocity.magnitude >= threshold) {
            Destroy(GameObject.Find("Door"));
            //possibility to trigger the opening of a prefab door through an animation
            //where the doors slowly disappear in the wall
        }
    }
}