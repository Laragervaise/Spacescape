using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayAudioOpenDoor : PlayAudio {
    /**
     * Play an audio clip and open the specified door after it.
     */
    public DSGates door;

    public override void DoAfterClip() {
        door.StartOpenClose();
    }
}
