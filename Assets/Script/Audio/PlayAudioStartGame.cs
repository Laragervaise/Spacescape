using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioStartGame : MonoBehaviour {
    public PlayAudio toBePlayed;
    // Start is called before the first frame update
    void Start() {
        StartCoroutine(toBePlayed.PlayClipAfterDelay(2));
    }
}
