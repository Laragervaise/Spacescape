using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioStartGame : MonoBehaviour {
    /**
     * Play an Audio clip (a PlayAudio) when the game starts, after 2 seconds
     */
    
    public PlayAudio toBePlayed;
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(toBePlayed.PlayClipAfterDelay(2));
    }
}
