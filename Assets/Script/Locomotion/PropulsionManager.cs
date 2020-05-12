﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    //Public instances
    public float _playerMass=20.0f;
    public float _bodySize = 1.0f;
    public float _damping = 0.001f;
    public float _minPlierSpeed = 0.1f;
    public double _minPlayerSpeed = 0.1;
    public float _vibrationDuration = 0.2f;
    public GameObject _leftController;
    public GameObject _rightController;

    //Private instances
    //Pliers
    private GameObject _leftHand;
    private GameObject _rightHand;

    //Controllers
    private ControllerGetter _leftControllerGetter;
    private ControllerGetter _rightControllerGetter;

    // Plier propellers
    private HandPropeller[] _handPropellers; //0: Left, 1: Right

    // State
    private bool _beingDragged = false;
    private int _draggingHand;
    private bool[] _handAttachedHeavy; // Hand attached to an object that can't be moved
    private bool _moving = false;
    private bool[] _forceRetract;      // Force retract the pliers
    private Vector3[] _handAttachedHeavyDirection;  //Direction of plier attachement point
    private Vector3 _goalDirection;                 //Camera Goal direction
    private Vector3 _lastPos;                       //Last position of the camera: used to compute _moving
    private Rigidbody _playerRigidBody;
    private Collider _playerCollider;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize variables
        _handAttachedHeavy = new bool[2]{false, false};
        _handAttachedHeavyDirection = new Vector3[2];
        _forceRetract = new bool[2]{false, false};
        _lastPos = this.transform.position;
        _leftHand = GameObject.Find("LeftPlier");
        _rightHand = GameObject.Find("RightPlier");
        _playerRigidBody = this.GetComponent<Rigidbody>();
        _playerRigidBody.isKinematic = false;
        _leftControllerGetter = _leftController.GetComponent<ControllerGetter>();
        _rightControllerGetter = _rightController.GetComponent<ControllerGetter>();

        _handPropellers = new HandPropeller[2]{_leftHand.GetComponent<HandPropeller>(),
                                               _rightHand.GetComponent<HandPropeller>()};

        _playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        /* For each plier, we have the following Behaviour:
            - If the player presses Extention input:
                If this hand is not already attached:
                  Start Propelling the plier.
            - If the player stops pressing Extention input:
                If this hand is not already attached:
                  Stop propelling the plier

            - If the player starts pressing retraction input:
                If the player is not already being dragged by any other hand (potentially remove this and apply to dragginghand only):
                  * If the hand is attached to an heavy object:
                      Attract the player to the heavy Object: retract toward the object
                  * If the hand is not attached to anything:
                      Retract the plier: retract toward the player
                  * TODO: If the hand is attach to a light Object:
                      Retract both the player and the object toward the center of mass of both objects. TODO: Do we freeze the plier during this action ?
            - If the player stops pressing retraction input:
                  * If it is attached to any object:
                      Stops dragging
                  * If the hand is not attached:
                      We only stop the retraction toward the object if there is no "force retract" of the plier toward the player (cancel operation)

            - If player hits the cancel Button:
                Force retract the plier and overwrites any other action

            - TODO: If the player presses the grab button:
                If the hand is colliding with an object
                    Attach the plier to the object and get the mass of this object
        */
        // LeftHand
        if(_leftControllerGetter.getCancel()) {
          OnCancelInput(0);
        } else {
            float plierSpeed = _leftControllerGetter.getPlierExtensionSpeed();
            if(plierSpeed>_minPlierSpeed  && !_beingDragged && !_handAttachedHeavy[0]) {
              OnExtension(0, plierSpeed);
            }
            if(Mathf.Abs(plierSpeed)<_minPlierSpeed) {
              if(!_beingDragged && !_handAttachedHeavy[0]) {
                OnExtensionStop(0);
              }
              if(!_handAttachedHeavy[0] && !_forceRetract[0]) {
                OnRetractionStop(0);
              } else {
                OnDragStop(0);
              }
            }
            if(plierSpeed<-_minPlierSpeed) {
              if(_handAttachedHeavy[0]) {
                  OnDragStart(_handAttachedHeavyDirection[0],0, -plierSpeed);
              } else {
                  OnRetraction(0, plierSpeed);
              }
            }
            if(_leftControllerGetter.getGrabbingStart()) {
              OnGrabStart(0);
            }
            if(_leftControllerGetter.getGrabbingStop()) {
              OnGrabStop(0);
            }
        }

        //RightHand
        if(_rightControllerGetter.getCancel()) {
          OnCancelInput(1);
        } else {
            float plierSpeed = _rightControllerGetter.getPlierExtensionSpeed();
            if(plierSpeed>_minPlierSpeed  && !_beingDragged && !_handAttachedHeavy[1]) {
              OnExtension(1, plierSpeed);
            }
            if(Mathf.Abs(plierSpeed)<_minPlierSpeed) {
              if(!_beingDragged && !_handAttachedHeavy[1]) {
                OnExtensionStop(1);
              }
              if(!_handAttachedHeavy[1] && !_forceRetract[1]) {
                OnRetractionStop(1);
              } else {
                OnDragStop(1);
              }
            }
            if(plierSpeed<-_minPlierSpeed) {
              if(_handAttachedHeavy[1]) {
                  OnDragStart(_handAttachedHeavyDirection[1],1, -plierSpeed);
              } else {
                  OnRetraction(1, plierSpeed);
              }
            }
            if(_rightControllerGetter.getGrabbingStart()) {
              OnGrabStart(1);
            }
            if(_rightControllerGetter.getGrabbingStop()) {
              OnGrabStop(1);
            }
        }
        // Update position
        //_lastPos = this.transform.position;

        //Movement Update
        if(_beingDragged) {
            if(Vector3.Distance(this.transform.position, _goalDirection) < _bodySize) {
                OnDragFinish(_draggingHand);
            }
        }
        if (_playerRigidBody.velocity.magnitude < _minPlayerSpeed) _playerRigidBody.velocity = Vector3.zero;
    }

    public Vector3 GetTranslation() {
         Vector3 trans = this.transform.position - _lastPos;
         _lastPos = this.transform.position;
         return trans;
    }

    private void OnCancelInput(int handType) {
        _handAttachedHeavy[handType] = false;
        _forceRetract[handType] = true;
        _handPropellers[handType].ForceRetractHand();
    }

    private void OnExtension(int handType, float speed) {
        _handPropellers[handType].PropulseHand(speed);
    }

    private void OnExtensionStop(int handType) {
        _handPropellers[handType].StopPropulseHand();
    }

    private void OnRetraction(int handType, float speed) {
        _handPropellers[handType].RetractHand(speed);
    }

    private void OnRetractionStop(int handType) {
        _handPropellers[handType].StopRetractHand();
    }

    private void OnGrabStart(int handType) {
        _handPropellers[handType].StartGrabbingHand();
    }

    private void OnGrabStop(int handType) {
        _handPropellers[handType].StopGrabbingHand();
        OnDragFinish(handType);
    }

    // If the player decided to drag toward an object
    private void OnDragStart(Vector3 goalDirection,int handType, float speed) {
        // Apply Force toward goal Direction
        if(!_beingDragged) {
            _beingDragged = true;
            _goalDirection = goalDirection;
            _draggingHand = handType;


            //If the other hand is attached to a wall, force retract it. Not if it is attacehd to an object.
            if(_handAttachedHeavy[(handType+1)%2]) {
                _handPropellers[(handType+1)%2].ForceRetractHand();
                _handAttachedHeavy[(handType+1)%2] = false;
            }
        }
        Vector3 forceDirection = goalDirection - this.transform.position;
        _playerRigidBody.velocity = Vector3.Normalize(forceDirection)*speed;
    }

    private void OnDragStop(int handType) {
        //Make velocity null
        if(_handAttachedHeavy[handType]) {
            _playerRigidBody.velocity = Vector3.zero;
        }
        _beingDragged = false;
    }

    private void OnDragFinish(int handType) {
          _handAttachedHeavy[handType] = false;
          endedForceRetraction(handType);
          _beingDragged = false;
    }

    // If a plier has attached a too heavy object to move
    public void OnAttachedPlierObject(Vector3 goalDirection,int handType, bool heavy) {
        _playerRigidBody.velocity = Vector3.zero;
        if(handType == 0) {
            _handAttachedHeavy[0] = heavy;
            _handAttachedHeavyDirection[0] = goalDirection;
            _leftControllerGetter.SetControllerVibrationOn(_vibrationDuration);
        }
        else {
            _handAttachedHeavy[1] = heavy;
            _handAttachedHeavyDirection[1] = goalDirection;
            _rightControllerGetter.SetControllerVibrationOn(_vibrationDuration);
        }
    }

    // This is being called when the propeller has reached its initial position.
    public void endedForceRetraction(int handType) {
        _forceRetract[handType] = false;
        _handAttachedHeavy[handType] = false;
    }

    //TODO: ISSUE HERE DOES NOT ENTER IF KINEMATIC MODE IS ON. BUT IF IT IS ON: PLIERS CAN PUSH THE PLAYER
    public void OnCollisionEnter(Collision other) {
        if(other.transform.tag  != "Plier") {
            Vector3 normal = other.GetContact(0).normal;
            _playerRigidBody.velocity = Vector3.Reflect(_playerRigidBody.velocity, normal) * _damping;
        }
    }
}
