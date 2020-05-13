using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayAudioOpenDoor : PlayAudio {
    public DSGates door;

    public override void DoAfterClip() {
        door.StartOpenClose();
    }
}
