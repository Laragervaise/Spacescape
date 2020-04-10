﻿﻿using System;
  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsCollision : MonoBehaviour {
    private Vector3 _lastPosition;
    
    // Start is called before the first frame update
    void Start()
    {    
        _lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        _lastPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision) {
        Vector3 normal = collision.contacts[0].normal;
        Vector3 vel = GetComponent<Rigidbody>().velocity;

        GetComponent<Rigidbody>().velocity = -vel;

        // float angle = Vector3.Angle(vel, -normal);
        // GetComponent<Rigidbody>().velocity = Vector3.Reflect(vel, normal);
    }

    public float maxAngle = 95;
}
