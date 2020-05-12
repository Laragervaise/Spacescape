using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    // Trigger the end of the game when access card is brought
    public GameObject accessCard;
    public GameObject faderAnchor;
    private OVRScreenFade fader;
    private bool triggered = false;

    void Start() {
        fader = faderAnchor.GetComponent<OVRScreenFade>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == accessCard && !triggered) {
            triggered = true;
            fader.FadeIn();
        }
    }
}
