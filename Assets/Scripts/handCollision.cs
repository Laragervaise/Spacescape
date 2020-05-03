using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handCollision : MonoBehaviour {
    //Player
    public GameObject _owner;
    public float _maxSpeed = 3.0f ;


    //private instances
    private Vector3 _lastPosition;
    private Vector3 _lastPositionLocal;
    private Vector3 _lastPositionOwner;
    private Vector3 _speed;
    private Vector3 _speedLocal;

    // Start is called before the first frame update
    void Start() {
        _lastPosition = transform.position;
        _lastPositionLocal = transform.localPosition;
        _lastPositionOwner = _owner.transform.position;
        _speed = Vector3.zero;
    }

    // Update is called once per frame
    void Update() {
        _lastPositionOwner = _owner.transform.position;

        Vector3 position = transform.position;
        Vector3 positionLocal = transform.localPosition;
        _speed = (position - _lastPosition)/Time.deltaTime;
        _speedLocal = (positionLocal - _lastPositionLocal)/Time.deltaTime;

        _lastPosition = position;
        _lastPositionLocal = positionLocal;

        //Debug.Log("speed: "+speed.magnitude);
        //Debug.Log("speed local: "+speedLocal.magnitude);
        //Debug.Log("\n");
    }

    void OnCollisionEnter(Collision collision) {

        if((collision.collider.gameObject.tag !="Player") & (collision.collider.gameObject.tag !="Plier")) {
            Vector3 normal = collision.contacts[0].normal;
            //Vector3 normal = -this.transform.forward;
            float magnitude = Vector3.Project(_speedLocal + _owner.GetComponent<Rigidbody>().velocity, normal).magnitude*0.5f;
            magnitude = Mathf.Min(magnitude, _maxSpeed);
            _owner.GetComponent<Rigidbody>().velocity = normal * (magnitude * 1.0f);
            //_owner.transform.position = _lastPositionOwner + collision.contacts[0].normal * (magnitude * Time.deltaTime * 2);
        }

    }
}
