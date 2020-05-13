using UnityEngine;

public class PlayAudioStartGame : MonoBehaviour {
    /**
     * Play an Audio clip (a PlayAudio) when the game starts, after 2 seconds
     */
    public PlayAudio toBePlayed;

    private bool _wasPressingA = false;


    void Start() {
        StartCoroutine(toBePlayed.PlayClipAfterDelay(2));
    }

    /**
     * Restart current audio (or last one if none is playing) by pressing 'A'
     */
    private void Update() {
        // Replay last audio source
        bool isPressingDown = OVRInput.Get(OVRInput.Button.One);
        if (isPressingDown && !_wasPressingA) {
            _wasPressingA = true;
            PlayAudio.PlayLastAudio();
        }
        else if (!isPressingDown) {
            _wasPressingA = false;
        }
    }
}