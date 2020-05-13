using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAlongGrabbables : MonoBehaviour {
    /**
     * This script makes the Grabble objects touching the trigger of this object move along when this
     * object is moved by hand. As long as it is grabbed by the player, every grabbable object touching
     * the trigger zone of this one will move along.
     * 
     * Allowing a cardboard box to move along with the items inside of it.
     */

    // Deal with audio trigger when all objects needed are here
    public GameObject[] objectsForTriggerAudio;
    private HashSet<GameObject> objectsForTriggerAudioSet;
    public PlayAudio audioToPlay;
    private bool hasAudioPlayed = false;
    
    
    private HashSet<GameObject> objectsMovingAlong = new HashSet<GameObject>();

    // Start is called before the first frame update
    void Start() {
        objectsForTriggerAudioSet = new HashSet<GameObject>(objectsForTriggerAudio);
    }

    // Update is called once per frame
    void FixedUpdate() {
        foreach (GameObject elem in objectsMovingAlong) {
            // if the object is not grabbed, put it as a child of MoveAlongGrabbables's gameobject
            if (!elem.GetComponent<Grabbable>()._is_grabbed) {
                elem.transform.parent = transform;
            }
            else {
                elem.transform.parent = null;
            }
        }
    }

    /*
     * Register the objects entering, adding them to the currently held objects
     */
    private void OnTriggerEnter(Collider other) {
        GameObject newObject = other.gameObject;
        Grabbable newGrabbable = newObject.GetComponent<Grabbable>();
        if (newGrabbable != null) {
            objectsMovingAlong.Add(newObject);
            CheckTriggerAudio();
        }
    }
    
    /*
     * Unregister the objects leaving, removing them for the currently held objects
     */
    private void OnTriggerExit(Collider other) {
        if (objectsMovingAlong.Contains(other.gameObject)) {
            objectsMovingAlong.Remove(other.gameObject);
            other.gameObject.transform.parent = null;
        }
    }

    /*
     * When the desired objects are all here, trigger an audio clip.
     */
    private void CheckTriggerAudio() {
        if (!hasAudioPlayed 
            && objectsForTriggerAudio != null 
            && objectsMovingAlong.IsSupersetOf(objectsForTriggerAudioSet)) {
            hasAudioPlayed = true;
            if (audioToPlay != null) {
                StartCoroutine(audioToPlay.PlayClip());
            }
        }
    }
}