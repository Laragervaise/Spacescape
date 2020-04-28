using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handCollision : MonoBehaviour {
    public GameObject rig;
    private Vector3 lastPosition;
    private Vector3 lastPosition2;
    private Vector3 lastPositionRig;
    private Vector3 speed;

    // Start is called before the first frame update
    void Start() {
        lastPosition = transform.position;
        lastPosition2 = transform.localPosition;
        speed = Vector3.zero;
    }

    // Update is called once per frame
    void Update() {
        lastPositionRig = rig.transform.position;

        Vector3 position = transform.position;
        Vector3 position2 = transform.localPosition;
        Vector3 tempSpeed = (position - lastPosition)/Time.deltaTime;
        speed = (position - lastPosition)/Time.deltaTime;
        Vector3 speed2 = (position2 - lastPosition2)/Time.deltaTime;
        lastPosition = position;
        lastPosition2 = position2;
        Debug.Log("speed: "+speed.magnitude);
        Debug.Log("speed local: "+speed2.magnitude);
        Debug.Log("\n");
        //if(speed.magnitude > 2)
            //Debug.Log(""+speed.magnitude);
    }

    private int i = 0;
    void OnCollisionEnter(Collision collision) {
        //Vector3 position = rig.transform.position;
        //Vector3 dPosition = position - lastPositionRig;
        //Debug.Log(dPosition.ToString("F4"));

        

        Vector3 normal = collision.contacts[0].normal;
        //Debug.Log(i++);
        //Debug.Log(speed.magnitude);
        //Debug.Log(rig.GetComponent<Rigidbody>().velocity.magnitude);
        float magnitude = Vector3.Project(speed + rig.GetComponent<Rigidbody>().velocity, normal).magnitude;
        //Debug.Log(magnitude);
        rig.GetComponent<Rigidbody>().velocity += normal * (magnitude * 1f);
        rig.transform.position = lastPositionRig + collision.contacts[0].normal * (magnitude * Time.deltaTime * 2);
    }
}
