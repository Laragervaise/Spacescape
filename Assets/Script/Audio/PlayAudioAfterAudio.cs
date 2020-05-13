using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioAfterAudio : PlayAudio {
    /*
     * Play another audio clip (PlayAudio) after this audio clip ended
     */
    
    public PlayAudio other;

    public override void DoAfterClip() {
        StartCoroutine(other.PlayClipAfterDelay(2.5f));
    }
}
