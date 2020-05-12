using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    // Trigger the end of the game when access card is brought
    public GameObject accessCard;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == accessCard && !triggered) {
            triggered = true;
            // TODO END ANIMATION
        }
    }
}
