﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    //Public instances
    public float _playerMass=20.0f;
    public float _bodySize = 1.0f;
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

        _leftControllerGetter = _leftController.GetComponent<ControllerGetter>();
        _rightControllerGetter = _rightController.GetComponent<ControllerGetter>();

        _handPropellers = new HandPropeller[2]{_leftHand.GetComponent<HandPropeller>(),
                                               _rightHand.GetComponent<HandPropeller>()};
    }

    void FixedUpdate()
    {
        /* For each plier, we have the following Behaviour:
            - If the player presses Extention input:
                If the player is not already being dragged on any other hand (potentially remove this) and this hand is not already attached:
                  Start Propelling the plier
            - If the player stops pressing Extention input:
                If the player is not already being dragged on any other hand and this hand not already attached (to be consistent with above):
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

            - TODO: If the player presses the grab button :
                If the hand is colliding with an object
                    Attach the plier to the object and get the mass of this object
        */
        // LeftHand
        if(_leftControllerGetter.getCancel()) {
          _handPropellers[0].RetractHand();
          _handAttachedHeavy[0] = false;
          _forceRetract[0] = true;
        } else {
          if(_leftControllerGetter.getExtensionStart() && !_beingDragged && !_handAttachedHeavy[0]) {
            _handPropellers[0].PropulseHand();
          }
          if(_leftControllerGetter.getExtensionStop() && !_beingDragged && !_handAttachedHeavy[0]) {
            _handPropellers[0].StopPropulseHand();
          }
          if(_leftControllerGetter.getRetractionStart() && !_beingDragged) {
            if(_handAttachedHeavy[0]) {
                OnDragOwner(_handAttachedHeavyDirection[0],0);
            } else {
                _handPropellers[0].RetractHand();
            }
          }
          if(_leftControllerGetter.getRetractionStop()) {
            if(!_handAttachedHeavy[0] && !_forceRetract[0]) {
                _handPropellers[0].StopRetractHand();
            } else {
                _beingDragged = false;
            }
          }
        }
        //RightHand
        if(_rightControllerGetter.getCancel()) {
          _handPropellers[1].RetractHand();
          _handAttachedHeavy[1] = false;
          _forceRetract[1] = true;
        } else {
          if(_rightControllerGetter.getExtensionStart() && !_beingDragged && !_handAttachedHeavy[1]) {
            _handPropellers[1].PropulseHand();
          }
          if(_rightControllerGetter.getExtensionStop() && !_beingDragged && !_handAttachedHeavy[1]) {
            _handPropellers[1].StopPropulseHand();
          }
          if(_rightControllerGetter.getRetractionStart() && !_beingDragged) {
            if(_handAttachedHeavy[1]) {
                OnDragOwner(_handAttachedHeavyDirection[1],1);
            } else {
                _handPropellers[1].RetractHand();
            }
          }
          if(_rightControllerGetter.getRetractionStop()) {
            if(!_handAttachedHeavy[1] && !_forceRetract[1]) {
                _handPropellers[1].StopRetractHand();
            } else {
              _beingDragged = false;
            }
          }
        }

        _moving = isPlayerMoving();

        //Movement if player is being dragged
        /* We drag the player if there is a non-null goal direction and :
              - it is being dragged
            OR
              - it is not being dragged, no plier is attached and the player is moving
                  => Basically conserves the speed of the player when it as detached his plier while retracting
        */
        if((_goalDirection != Vector3.zero) &&
                (_beingDragged ||
                    (!_beingDragged && !_handAttachedHeavy[0] && !_handAttachedHeavy[1] && _moving)
                )
           )
        {
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
                    } else {
                        _handAttachedHeavy[1] = false;
                    }
                }
                _goalDirection = Vector3.zero;
            }
        }
    }

    // See if player moved the last frame
    private bool isPlayerMoving() {
      float displacement = Vector3.Distance(this.transform.position, _lastPos);
      _lastPos = this.transform.position;

      return displacement>0.001f;
    }

    // If a plier has attached a too heavy object to move
    public void OnAttachedPlierHeavier(Vector3 goalDirection,int handType) {
        if(handType == 0) {
            _handAttachedHeavy[0] = true;
            _handAttachedHeavyDirection[0] = goalDirection;
            _leftControllerGetter.SetControllerVibrationOn(0.2f);
        }
        else {
            _handAttachedHeavy[1] = true;
            _handAttachedHeavyDirection[1] = goalDirection;
        }
    }

    // If the player decided to drag toward an object
    public void OnDragOwner(Vector3 goalDirection,int handType) {
        _beingDragged = true;
        _goalDirection = goalDirection;
        _draggingHand = handType;
        _handPropellers[(handType+1)%2].RetractHand();
        if(handType == 0) {
            _handAttachedHeavy[1] = false;
        } else {
            _handAttachedHeavy[0] = false;
        }

    }

    // This is being called when the propeller has reached its initial position.
    public void endedHandRetraction(int handType) {
        _forceRetract[handType] = false;
    }
}
