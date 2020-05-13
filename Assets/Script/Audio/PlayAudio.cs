using System.Collections;
using System.Threading.Tasks;
using Substance.Game;
using UnityEngine;

[System.Serializable]
public abstract class PlayAudio : MonoBehaviour {
    private AudioSource audio;
    private bool alreadyPlayed = false;

    public void Start() {
        audio = GetComponent<AudioSource>();
    }

    public IEnumerator PlayClipAfterDelay(float seconds) {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(PlayClip());
    }
    
    public IEnumerator PlayClip() {
        if (!alreadyPlayed) {
            alreadyPlayed = true;
            audio.Play();
            
            yield return new WaitForSeconds(audio.clip.length);
            DoAfterClip();
        }
    }

    public abstract void DoAfterClip();
}