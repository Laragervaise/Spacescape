﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    //Public instances
    public float _playerMass=20.0f;
    public GameObject _leftController;
    public GameObject _rightController;

    //Private instances
    private GameObject _leftHand;
    private GameObject _rightHand;

    private ControllerGetter _leftControllerGetter;
    private ControllerGetter _rightControllerGetter;

    private HandPropeller[] _handPropellers; //0: Left, 1: Right

    private float _bodySize = 1.0f;
    private bool _beingDragged = false;
    private int _draggingHand;
    private bool[] _handAttachedHeavy;
    //private bool _leftHandAttachedHeavy = false;
    //private bool _rightHandAttachedHeavy= false;
    private bool _moving = false;
    private bool[] _forceRetract;
    //private Vector3 _leftHandAttachedHeavyDirection;
    //private Vector3 _rightHandAttachedHeavyDirection;
    private Vector3[] _handAttachedHeavyDirection;
    private Vector3 _goalDirection;
    private Vector3 _lastPos;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogWarning("Entering Start PropulsionManager");
        _handAttachedHeavy = new bool[2]{false, false};
        _handAttachedHeavyDirection = new Vector3[2];
        _forceRetract = new bool[2]{false, false};
        _lastPos = this.transform.position;
        _leftHand = GameObject.Find("LeftPlier");
        _rightHand = GameObject.Find("RightPlier");

        _leftControllerGetter = _leftController.GetComponent<ControllerGetter>();
        _rightControllerGetter = _rightController.GetComponent<ControllerGetter>();

        Debug.LogWarning("Found hands PropulsionManager");
        _handPropellers = new HandPropeller[2]{_leftHand.GetComponent<HandPropeller>(),
                                               _rightHand.GetComponent<HandPropeller>()};
        Debug.LogWarning("Exit Start PropulsionManager");
        //StartCoroutine("BeginPropelHand");
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // LeftHand
        if(Input.GetKeyDown("e")) {
          _handPropellers[0].RetractHand();
          _handAttachedHeavy[0] = false;
          _forceRetract[0] = true;
        } else {
          // TODO : Allow other hand to extend
          if(Input.GetKeyDown("a") && !_beingDragged && !_handAttachedHeavy[0]) {
            _handPropellers[0].PropulseHand();
          }
          if(Input.GetKeyUp("a") && !_beingDragged && !_handAttachedHeavy[0]) {
            _handPropellers[0].StopPropulseHand();
          }
          if(Input.GetKeyDown("z") && !_beingDragged) {
            if(_handAttachedHeavy[0]) {
                OnDragOwner(_handAttachedHeavyDirection[0],0);
            } else {
                _handPropellers[0].RetractHand();
            }
          }
          if(Input.GetKeyUp("z")) {
            if(!_handAttachedHeavy[0] && !_forceRetract[0]) {
                _handPropellers[0].StopRetractHand();
            } else {
                _beingDragged = false;
            }
          }
        }
        //RightHand
        if(Input.GetKeyDown("i")) {
          _handPropellers[1].RetractHand();
          _handAttachedHeavy[1] = false;
          _forceRetract[1] = true;
        } else {
          if(Input.GetKeyDown("p") && !_beingDragged && !_handAttachedHeavy[1]) {
            _handPropellers[1].PropulseHand();
          }
          if(Input.GetKeyUp("p") && !_beingDragged && !_handAttachedHeavy[1]) {
            _handPropellers[1].StopPropulseHand();
          }
          if(Input.GetKeyDown("o") && !_beingDragged) {
            if(_handAttachedHeavy[1]) {
                OnDragOwner(_handAttachedHeavyDirection[1],1);
            } else {
                _handPropellers[1].RetractHand();
            }
          }
          if(Input.GetKeyUp("o")) {
            if(!_handAttachedHeavy[1] && !_forceRetract[1]) {
                _handPropellers[1].StopRetractHand();
            } else {
              _beingDragged = false;
            }
          }
        }
        */

        /*
        // LeftHand

        // If the player wants to propulse left plier, that the player is not being dragged and the plier is not already attached
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].PropulseHand();
        }
        // If the player stops propulsing left plier, stop its movement.
        if(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].StopPropulseHand();
        }
        // If the player decides the retract the plier: Drags the player if hand is attached, retract the plier otherwise
        if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && !_beingDragged) {
          if(_leftHandAttachedHeavy) {
              OnDragOwner(_leftHandAttachedHeavyDirection,0);
          } else {
              _handPropellers[0].RetractHand();
          }
        }
        // If the player stops retracting the plier, it is not being dragged anymore if he was being dragged, stop retracting otherwise.
        if(OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger)) {
          if(!_leftHandAttachedHeavy) {
              _handPropellers[0].StopRetractHand();
          } else {
              _beingDragged = false;
          }

        }

        //RightHand
        // If the player wants to propulse right plier, that the player is not being dragged and the plier is not already attached
        if(OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].PropulseHand();
        }
        // If the player stops propulsing left plier, stop its movement.
        if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].StopPropulseHand();
        }
        // If the player decides the retract the plier: Drags the player if hand is attached, retract the plier otherwise
        if(OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) && !_beingDragged) {
          if(_rightHandAttachedHeavy) {
              OnDragOwner(_rightHandAttachedHeavyDirection,1);
          } else {
              _handPropellers[1].RetractHand();
          }
        }
        // If the player stops retracting the plier, it is not being dragged anymore if he was being dragged, stop retracting otherwise.
        if(OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger)) {
          if(!_rightHandAttachedHeavy) {
              _handPropellers[1].StopRetractHand();
          } else {
              _beingDragged = false;
          }
        }*/

        // LeftHand
        if(_leftControllerGetter.getCancel()) {
          _handPropellers[0].RetractHand();
          _handAttachedHeavy[0] = false;
          _forceRetract[0] = true;
        } else {
          // TODO : Allow other hand to extend
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
        if((_goalDirection != Vector3.zero) &&
                (_beingDragged ||
                    (!_beingDragged && !_handAttachedHeavy[0] && !_handAttachedHeavy[1] && _moving)
                )
           ) {
            if(Vector3.Distance(this.transform.position, _goalDirection) > _bodySize) {
                this.transform.position = Vector3.MoveTowards(transform.position, _goalDirection, Time.deltaTime);
                if(_beingDragged) _handPropellers[_draggingHand].FreezePositionOnDrag();
            } else {
                if(_beingDragged) {
                    _handPropellers[_draggingHand].OnDragFinished();
                    _beingDragged = false;
                    if(_draggingHand == 0){
                        _handAttachedHeavy[0] = false;
                    } else {
                        _handAttachedHeavy[1] = false;
                    }
                    print("PLIER DETACHED " + _draggingHand);
                }
                _goalDirection = Vector3.zero;
            }
        }
    }

    private bool isPlayerMoving() {
      float displacement = Vector3.Distance(this.transform.position, _lastPos);
      _lastPos = this.transform.position;

      return displacement>0.001f;
    }

    public void OnAttachedPlierHeavier(Vector3 goalDirection,int handType) {
        print("PLIER ATTACHED: "+handType);
        if(handType == 0) {
            _handAttachedHeavy[0] = true;
            _handAttachedHeavyDirection[0] = goalDirection;
        }
        else {
            _handAttachedHeavy[1] = true;
            _handAttachedHeavyDirection[1] = goalDirection;
        }
    }

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

    public void endedHandRetraction(int handType) {
        _forceRetract[handType] = false;
    }
}
