﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    /*
        PROPULSION MANAGER

        Provides the interface for the robot's body movements.
        It collects inputs from both controllers, triggers action for the pliers accordingly, and provide any reaction to the robot's Body


        Attach to GameObject : Player
    */


    //Public instances
    public float _playerMass=20.0f;
    public float _bodySize = 1.0f;
    public float _damping = 0.1f;
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
    private Rigidbody _playerRigidBody;
    private Collider _playerCollider;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize variables
        _handAttachedHeavy = new bool[2]{false, false};
        _handAttachedHeavyDirection = new Vector3[2];
        _forceRetract = new bool[2]{false, false};
        _leftHand = GameObject.Find("LeftPlier");
        _rightHand = GameObject.Find("RightPlier");
        _playerRigidBody = this.GetComponent<Rigidbody>();
        _playerRigidBody.isKinematic = false;
        _leftControllerGetter = _leftController.GetComponent<ControllerGetter>();
        _rightControllerGetter = _rightController.GetComponent<ControllerGetter>();

        _handPropellers = new HandPropeller[2]{_leftHand.GetComponent<HandPropeller>(),
                                               _rightHand.GetComponent<HandPropeller>()};

        _playerRigidBody.constraints = RigidbodyConstraints.FreezeRotation;

        // Retract hands at the beginning as they are not always by default
        OnCancelInput(0);
        OnCancelInput(1);
    }

    void FixedUpdate()
    {
        /* For each plier, we have the following Behaviour:
            - If the player presses Extention input:
                If this hand is not already attached:
                  Start Propelling the plier.

            - If the player starts pressing retraction input:
                If the player is attached to a heavy object:
                  Attracts the player to the heavy Object: retract toward the object
                Else (attached to a light object or not attached):
                  Retract the plier: retract toward the player

            - If player hits the cancel Button:
                Force retract the plier and overwrites any other action

            - If the player presses the grab button:
                If the hand is colliding with an object
                    Attach the plier to the object
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

        //Movement Update
        if(_beingDragged) {
            if(Vector3.Distance(this.transform.position, _goalDirection) < _bodySize) {
                OnDragFinish(_draggingHand);
            }
        }

        //Zero Velocity when too small
        if (_playerRigidBody.velocity.magnitude < _minPlayerSpeed) _playerRigidBody.velocity = Vector3.zero;
    }

    // If cancel: Distach and Force Retract the corresponding plier
    private void OnCancelInput(int handType) {
        _handAttachedHeavy[handType] = false;
        _forceRetract[handType] = true;
        _handPropellers[handType].ForceRetractHand();
    }

    //  If extend: Propulse hand with speed according to joystick pressure
    private void OnExtension(int handType, float speed) {
        _handPropellers[handType].PropulseHand(speed);
    }

    // If stop extending: Set speed of pliers to 0
    private void OnExtensionStop(int handType) {
        _handPropellers[handType].StopPropulseHand();
    }

    // If retracting: *retract hand with speed according to joystick
    private void OnRetraction(int handType, float speed) {
        _handPropellers[handType].RetractHand(speed);
    }

    // If stop retracting: Set speed of pliers to 0
    private void OnRetractionStop(int handType) {
        _handPropellers[handType].StopRetractHand();
    }

    // If grabbing: Update state of plier
    private void OnGrabStart(int handType) {
        _handPropellers[handType].StartGrabbingHand();
    }

    private void OnGrabStop(int handType) {
        _handPropellers[handType].StopGrabbingHand();
        OnDragFinish(handType);
    }

    // If the player decided to drag toward an object
    private void OnDragStart(Vector3 goalDirection,int handType, float speed) {
        // Apply velocity toward goal Direction
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

    // On drag stop: Set speed to 0 only if it is attached to a heavy object
    private void OnDragStop(int handType) {
        //Make velocity null
        if(_handAttachedHeavy[handType]) {
            _playerRigidBody.velocity = Vector3.zero;
        }
        _beingDragged = false;
    }

    // On Drag finish: Distach the hand
    private void OnDragFinish(int handType) {
          _handAttachedHeavy[handType] = false;
          endedForceRetraction(handType);
          _beingDragged = false;
    }

    // Called from OnCollisionEnter of the pliers, update goal direction and starts vibration
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

    // Slow bouncing effect when colliding with a wall.
    public void OnCollisionEnter(Collision other) {
        if(other.transform.tag  != "Plier") {
            Vector3 normal = other.GetContact(0).normal;
            _playerRigidBody.velocity = Vector3.Reflect(_playerRigidBody.velocity, normal) * _damping;
        }
    }
}
