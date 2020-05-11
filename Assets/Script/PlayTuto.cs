using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTuto : MonoBehaviour
{

    public AudioClip SoundToPlay;
    [Range(0, 5)]
    public float Volume = 0.5f;
    [Range(0, 5)]
    public float duration = 3f;
    AudioSource audio;
    private bool alreadyPlayed = false;

    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter()
    {
        if (!alreadyPlayed)
        {
            audio.PlayOneShot(SoundToPlay, Volume);
            alreadyPlayed = true;
        } 
        else if (OVRInput.Get(OVRInput.Button.One))
        {
            audio.PlayOneShot(SoundToPlay, Volume);
            StartCoroutine(Break(duration));
        }
    }

    IEnumerator Break(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

}
