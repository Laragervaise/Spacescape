using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioAfterAudio : PlayAudio {
    public PlayAudio other;

    public override void DoAfterClip() {
        StartCoroutine(other.PlayClip());
    }
}
