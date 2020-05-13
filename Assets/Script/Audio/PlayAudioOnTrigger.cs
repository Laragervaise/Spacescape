using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioOnTrigger : PlayAudio {
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