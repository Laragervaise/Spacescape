﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    //Public instances
    public float _playerMass=20.0f;
    public float _bodySize = 1.0f;
    public float _damping = 0.1f;
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
          if(_leftControllerGetter.getExtensionStart() && !_beingDragged && !_handAttachedHeavy[0]) {
            OnExtensionStart(0);
          }
          if(_leftControllerGetter.getExtensionStop() && !_beingDragged && !_handAttachedHeavy[0]) {
            OnExtensionStop(0);
          }
          if(_leftControllerGetter.getRetractionStart() && !_beingDragged) {
            if(_handAttachedHeavy[0]) {
                OnDragStart(_handAttachedHeavyDirection[0],0);
            } else {
                OnRetractionStart(0);
            }
          }
          if(_leftControllerGetter.getRetractionStop()) {
            if(!_handAttachedHeavy[0] && !_forceRetract[0]) {
                OnRetractionStop(0);
            } else {
                OnDragStop(0);
            }
          }
        }
        //RightHand
        if(_rightControllerGetter.getCancel()) {
            OnCancelInput(1);
        } else {
          if(_rightControllerGetter.getExtensionStart() && !_beingDragged && !_handAttachedHeavy[1]) {
            OnExtensionStart(1);
          }
          if(_rightControllerGetter.getExtensionStop() && !_beingDragged && !_handAttachedHeavy[1]) {
            OnExtensionStop(1);
          }
          if(_rightControllerGetter.getRetractionStart() && !_beingDragged) {
            if(_handAttachedHeavy[1]) {
                OnDragStart(_handAttachedHeavyDirection[1],1);
            } else {
                OnRetractionStart(1);
            }
          }
          if(_rightControllerGetter.getRetractionStop()) {
            if(!_handAttachedHeavy[1] && !_forceRetract[1]) {
                OnRetractionStop(1);
            } else {
                OnDragStop(1);
            }
          }
        }

        _moving = isPlayerMoving();

        if(_beingDragged) {
            if(Vector3.Distance(this.transform.position, _goalDirection) < _bodySize) {
                OnDragStop(_draggingHand);
                _handPropellers[_draggingHand].OnDragFinished();
                if(_draggingHand == 0){
                    _handAttachedHeavy[0] = false;
                    endedForceRetraction(0);
                } else {
                    _handAttachedHeavy[1] = false;
                    endedForceRetraction(0);
                }
            }
        }
        if (_playerRigidBody.velocity.magnitude < 0.1) _playerRigidBody.velocity = Vector3.zero;
        //Movement if player is being dragged
        /* We drag the player if there is a non-null goal direction and :
              - it is being dragged
            OR
              - it is not being dragged, no plier is attached and the player is moving
                  => Basically conserves the speed of the player when it as detached his plier while retracting
        */
        /*
        if((_goalDirection != Vector3.zero) &&
                (_beingDragged ||
                    (!_beingDragged && !_handAttachedHeavy[0] && !_handAttachedHeavy[1] && _moving)
                )
           ) {
            // If the distance between the goal diraction and the player is big enough: move the player toward the position
            if(Vector3.Distance(this.transform.position, _goalDirection) > _bodySize) {
                this.transform.position = Vector3.MoveTowards(transform.position, _goalDirection, Time.deltaTime);
            }
              // Else : if it was being dragged: finish the dragging operation, and in both cases (was moving), remove goal diraction: stops motion
              else {
                if(_beingDragged) {
                    _handPropellers[_draggingHand].OnDragFinished();
                    _beingDragged = false;
                    if(_draggingHand == 0){
                        _handAttachedHeavy[0] = false;
                        endedForceRetraction(0);
                    } else {
                        _handAttachedHeavy[1] = false;
                        endedForceRetraction(1);
                    }
                }
                _goalDirection = Vector3.zero;
            }
        }*/
    }

    // See if player moved the last frame
    private bool isPlayerMoving() {
      float displacement = Vector3.Distance(this.transform.position, _lastPos);
      _lastPos = this.transform.position;

      return displacement>0.001f;
    }

    private void OnCancelInput(int handType) {
        _handAttachedHeavy[handType] = false;
        _forceRetract[handType] = true;
        _handPropellers[handType].ForceRetractHand();
    }

    private void OnExtensionStart(int handType) {
        _handPropellers[handType].PropulseHand();
    }

    private void OnExtensionStop(int handType) {
        _handPropellers[handType].StopPropulseHand();
    }

    private void OnRetractionStart(int handType) {
        _handPropellers[handType].RetractHand();
    }

    private void OnRetractionStop(int handType) {
        _handPropellers[handType].StopRetractHand();
    }

    // If the player decided to drag toward an object
    private void OnDragStart(Vector3 goalDirection,int handType) {
        // Apply Force toward goal Direction
        _beingDragged = true;
        _goalDirection = goalDirection;
        _draggingHand = handType;
        _handPropellers[(handType+1)%2].RetractHand();
        Vector3 forceDirection = goalDirection - this.transform.position;
        print(forceDirection + " " + Time.frameCount);
        _playerRigidBody.velocity = Vector3.Normalize(forceDirection);
        if(handType == 0) {
            _handAttachedHeavy[1] = false;
        } else {
            _handAttachedHeavy[0] = false;
        }
    }

    private void OnDragStop(int handType) {
        //Make velocity null
        if(_handAttachedHeavy[handType]) {
            _playerRigidBody.velocity = Vector3.zero;
        }
        _beingDragged = false;

    }

    // If a plier has attached a too heavy object to move
    public void OnAttachedPlierHeavier(Vector3 goalDirection,int handType) {
        _playerRigidBody.velocity = Vector3.zero;
        if(handType == 0) {
            _handAttachedHeavy[0] = true;
            _handAttachedHeavyDirection[0] = goalDirection;
            _leftControllerGetter.SetControllerVibrationOn(0.2f);
        }
        else {
            _handAttachedHeavy[1] = true;
            _handAttachedHeavyDirection[1] = goalDirection;
            _rightControllerGetter.SetControllerVibrationOn(0.2f);
        }
    }

    // This is being called when the propeller has reached its initial position.
    public void endedForceRetraction(int handType) {
        _forceRetract[handType] = false;
    }

    public void OnCollisionEnter(Collision other) {
        if(other.transform.tag  == "Heavy") {
            Vector3 normal = other.GetContact(0).normal;
            _playerRigidBody.velocity = Vector3.Reflect(_playerRigidBody.velocity, normal) * _damping;
        }
    }
}
