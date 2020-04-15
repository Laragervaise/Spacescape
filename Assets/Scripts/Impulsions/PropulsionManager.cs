﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropulsionManager : MonoBehaviour {

    //Public instances
    public float _playerMass=20.0f;

    //Private instances
    private GameObject _leftHand;
    private GameObject _rightHand;
    private OVRHand _leftOVRHand;
    private OVRHand _rightOVRHand;

    private HandPropeller[] _handPropellers; //0: Left, 1: Right

    private float _bodySize = 1.0f;
    private bool _beingDragged = false;
    private int _draggingHand;
    private bool _leftHandAttachedHeavy = false;
    private bool _rightHandAttachedHeavy= false;
    private Vector3 _leftHandAttachedHeavyDirection;
    private Vector3 _rightHandAttachedHeavyDirection;
    private Vector3 _goalDirection;


    // Start is called before the first frame update
    void Start()
    {
        _leftHand = GameObject.Find("LeftPlier");
        _rightHand = GameObject.Find("RightPlier");

        _handPropellers = new HandPropeller[2]{_leftHand.GetComponent<HandPropeller>(),
                                               _rightHand.GetComponent<HandPropeller>()};

        //StartCoroutine("BeginPropelHand");
    }

    // Update is called once per frame
    void Update()
    {   /*
        // LeftHand
        if(Input.GetKeyDown("a") && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].PropulseHand();
        }
        if(Input.GetKeyUp("a") && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].StopPropulseHand();
        }
        if(Input.GetKeyDown("z") && !_beingDragged) {
          if(_leftHandAttachedHeavy) {
              OnDragOwner(_leftHandAttachedHeavyDirection,0);
          } else {
              _handPropellers[0].RetractHand();
          }
        }
        if(Input.GetKeyUp("z")) {
          if(!_leftHandAttachedHeavy) {
              _handPropellers[0].StopRetractHand();
          } else {
              _beingDragged = false;
          }

        }

        //RightHand
        if(Input.GetKeyDown("p") && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].PropulseHand();
        }
        if(Input.GetKeyUp("p") && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].StopPropulseHand();
        }
        if(Input.GetKeyDown("o") && !_beingDragged) {
          if(_rightHandAttachedHeavy) {
              OnDragOwner(_rightHandAttachedHeavyDirection,1);
          } else {
              _handPropellers[1].RetractHand();
          }
        }
        if(Input.GetKeyUp("o")) {
          if(!_rightHandAttachedHeavy) {
              _handPropellers[1].StopRetractHand();
          } else {
              _beingDragged = false;
          }
        }
        */

        // LeftHand
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].PropulseHand();
        }
        if(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) && !_beingDragged && !_leftHandAttachedHeavy) {
          _handPropellers[0].StopPropulseHand();
        }
        if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && !_beingDragged) {
          if(_leftHandAttachedHeavy) {
              OnDragOwner(_leftHandAttachedHeavyDirection,0);
          } else {
              _handPropellers[0].RetractHand();
          }
        }
        if(OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger)) {
          if(!_leftHandAttachedHeavy) {
              _handPropellers[0].StopRetractHand();
          } else {
              _beingDragged = false;
          }

        }

        //RightHand
        if(OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].PropulseHand();
        }
        if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) && !_beingDragged && !_rightHandAttachedHeavy) {
          _handPropellers[1].StopPropulseHand();
        }
        if(OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) && !_beingDragged) {
          if(_rightHandAttachedHeavy) {
              OnDragOwner(_rightHandAttachedHeavyDirection,1);
          } else {
              _handPropellers[1].RetractHand();
          }
        }
        if(OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger)) {
          if(!_rightHandAttachedHeavy) {
              _handPropellers[1].StopRetractHand();
          } else {
              _beingDragged = false;
          }
        }
        //Movement
        if(_beingDragged) {
            if(Vector3.Distance(this.transform.position, _goalDirection) > _bodySize) {
                this.transform.position = Vector3.MoveTowards(transform.position, _goalDirection, Time.deltaTime);
                _handPropellers[_draggingHand].FreezePositionOnDrag();
            } else {
                _handPropellers[_draggingHand].OnDragFinished();
                _beingDragged = false;
                if(_draggingHand == 0){
                    _leftHandAttachedHeavy = false;
                } else {
                    _rightHandAttachedHeavy = false;
                }
                print("PLIER DETACHED " + _draggingHand);
            }
        }

    }

    public void OnAttachedPlierHeavier(Vector3 goalDirection,int handType) {
        print("PLIER ATTACHED: "+handType);
        if(handType == 0) {
            _leftHandAttachedHeavy = true;
            _leftHandAttachedHeavyDirection = goalDirection;
        }
        else {
            _rightHandAttachedHeavy = true;
            _rightHandAttachedHeavyDirection = goalDirection;
        }
    }

    public void OnDragOwner(Vector3 goalDirection,int handType) {
        //StartCoroutine(DragOwner(goalDirection, handType));
        _beingDragged = true;
        _goalDirection = goalDirection;
        _draggingHand = handType;
        _handPropellers[(handType+1)%2].RetractHand();
        if(handType == 0) {
            _rightHandAttachedHeavy = false;
        } else {
            _leftHandAttachedHeavy = false;
        }

    }
}
