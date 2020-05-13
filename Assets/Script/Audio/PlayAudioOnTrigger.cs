using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnTrigger : PlayAudio {
    /**
     * Play an audio clip upon entering this gameobject collider (trigger)
     * with a specified other gameobject.
     */
    
    public GameObject objectToDetect;

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == objectToDetect) {
            StartCoroutine(PlayClip());
        }
    }

    public override void DoAfterClip() {
        // Nothing
    }
}