using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HandPropeller : MonoBehaviour
{
    public enum AnchorSideType : int {
        LeftHand = 0,
        RightHand = 1
    };

    public float _propulsionSpeed = 1.0f;
    public float _initialOffset = 0.2f;

    public GameObject _owner;
    public AnchorSideType _anchorSide;

    private PropulsionManager _ownerPM;
    private Rigidbody _handRigidBody;
    private bool _propelling = false;
    private bool _retracting = false;
    private bool _attached = false;
    private Vector3 _initPos;
    private Vector3 _collisionPosition;
    private Quaternion _collisionOrientation;




    // Start is called before the first frame update
    void Start()
    {
        _handRigidBody = this.GetComponent<Rigidbody>();
        _handRigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        _ownerPM = _owner.GetComponent<PropulsionManager>();
        if((int)_anchorSide == 0) {
            this.transform.localRotation = Quaternion.Euler(0, -90, 0);
        } else {
            this.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        this.transform.position += this.transform.forward * _initialOffset;
        _initPos = this.transform.localPosition;

        _handRigidBody.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_propelling) {
            _handRigidBody.MovePosition(this.transform.position + this.transform.forward * Time.deltaTime * _propulsionSpeed);
        }
        if(_retracting) {
            this.transform.position = Vector3.MoveTowards(this.transform.position,
                                                          _owner.transform.position,
                                                          Time.deltaTime * _propulsionSpeed);

            if(Vector3.Distance(this.transform.position, _owner.transform.position) < (0.2f + 0.005)) {
                this.transform.localPosition = _initPos;
                _retracting = false;
                _ownerPM.endedHandRetraction((int) _anchorSide);
            }
        }
        if(_attached) {
            this.transform.position = _collisionPosition - this.transform.forward*0.05f;
            this.transform.rotation = _collisionOrientation;
        }
    }

    public void PropulseHand() {
        _propelling = true;
        _retracting = false;
        _attached = false;
    }

    public void StopPropulseHand() {
        _propelling = false;
        _retracting = false;
        _attached = false;
    }

    public void RetractHand() {
        _propelling = false;
        _retracting = true;
        _attached = false;
    }

    public void StopRetractHand() {
        _propelling = false;
        _retracting = false;
        _attached = false;
    }

    public void OnCollisionEnter(Collision other) {
        if(other.gameObject.tag != "NotDragable" && _propelling) {
            _propelling = false;
            _retracting = false;
            _attached = true;

            _handRigidBody.velocity = Vector3.zero;
            //_handRigidBody.transform.localPosition += Vector3.back*0.05f;
            _handRigidBody.Sleep();

            if(_ownerPM._playerMass < other.rigidbody.mass) {
              _ownerPM.OnAttachedPlierHeavier(other.GetContact(0).point, (int)_anchorSide);
            }
            _collisionPosition = other.GetContact(0).point;
            _collisionOrientation = this.transform.rotation;

        }
    }



    public void OnDragFinished() {
        _attached = false;
        _propelling = false;
        _retracting = false;
        _handRigidBody.WakeUp();

        this.transform.localPosition = _initPos;
    }

    public void FreezePositionOnDrag() {
      // Ensures the plier do not move the wall.
      // Yet let the plier retract if player decided to cancel the drag.
      if(!_retracting) {
        this.transform.position = (_collisionPosition - this.transform.forward*0.05f);
      }
    }
}
