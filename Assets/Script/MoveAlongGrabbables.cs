using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAlongGrabbables : MonoBehaviour {
    /**
     * This script makes the Grabble objects touching the trigger of this object move along.
     * Allowing a cardboard box to move along with the items inside of it.
     */

    // Deal with audio trigger when all objects needed are here
    public GameObject[] objectsForTriggerAudio;
    private HashSet<GameObject> objectsForTriggerAudioSet;
    public PlayAudio audioToPlay;
    private bool hasAudioPlayed = false;
    
    
    private HashSet<GameObject> objectsMovingAlong = new HashSet<GameObject>();

    private Vector3 lastPosition;
    private Vector3 lastRotation;

    // Start is called before the first frame update
    void Start() {
        objectsForTriggerAudioSet = new HashSet<GameObject>(objectsForTriggerAudio);
        lastPosition = transform.position;
        lastRotation = transform.eulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 dPosition = transform.position - lastPosition;
        Vector3 dRotation = transform.eulerAngles - lastRotation;
        
        foreach (GameObject elem in objectsMovingAlong) {
            if (!elem.GetComponent<Grabbable>()._is_grabbed) {
                elem.transform.parent = transform;
            }
            else {
                elem.transform.parent = null;
            }
        }

        lastPosition = transform.position;
        lastRotation = transform.eulerAngles;
    }

    private void OnTriggerEnter(Collider other) {
        GameObject newObject = other.gameObject;
        Grabbable newGrabbable = newObject.GetComponent<Grabbable>();
        if (newGrabbable != null) {
            objectsMovingAlong.Add(newObject);
            CheckTriggerAudio();
        }
    }
    
    private void OnTriggerExit(Collider other) {
        if (objectsMovingAlong.Contains(other.gameObject)) {
            objectsMovingAlong.Remove(other.gameObject);
            other.gameObject.transform.parent = null;
        }
    }

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