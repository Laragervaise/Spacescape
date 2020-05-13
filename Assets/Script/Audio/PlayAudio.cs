using System.Collections;
using System.Threading.Tasks;
using Substance.Game;
using UnityEngine;

[System.Serializable]
public abstract class PlayAudio : MonoBehaviour {
    /*
     * Deals with triggered audio in the game, either by event or collider
     *
     * Must be inherited to be implemented (abstract class).
     */
    private static AudioSource lastAudio;

    private AudioSource audio;
    private bool alreadyPlayed = false;

    public void Start() {
        audio = GetComponent<AudioSource>();
    }

    public static void PlayLastAudio() {
        if (lastAudio != null) {
            lastAudio.Stop();
            lastAudio.Play();
        }
    }

    /**
     * Play the clip from the audio source of this gameobject, after 'seconds' seconds
     * Only the first ever call to PlayClip will play the clip
     * Must be called using "StartCoroutine(PlayClipAfterDelay(seconds))"
     */
    public IEnumerator PlayClipAfterDelay(float seconds) {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(PlayClip());
    }

    /**
     * Play the clip from the audio source of this gameobject
     * Only the first ever call to PlayClip will play the clip
     * Must be called using "StartCoroutine(PlayClip())"
     */
    public IEnumerator PlayClip() {
        if (!alreadyPlayed) {
            alreadyPlayed = true;
            if (lastAudio != null) {
                lastAudio.Stop();
            }

            lastAudio = audio;
            audio.Play();

            yield return new WaitForSeconds(audio.clip.length);
            DoAfterClip();
        }
    }

    /**
     * Is called after the clip finished playing, must be implemented by child class
     */
    public abstract void DoAfterClip();
}