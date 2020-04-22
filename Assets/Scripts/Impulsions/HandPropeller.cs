using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HandPropeller : MonoBehaviour
{
    // Public variables
    public enum AnchorSideType : int {
        LeftHand = 0,
        RightHand = 1
    };
    public float _propulsionSpeed = 1.0f;
    public float _initialOffset = 0.2f;
    public GameObject _owner;
    public AnchorSideType _anchorSide;
    public GameObject _tubeCylinderPrefab;


    //Private instances
    private static float _initPropulsionSpeed;

    private PropulsionManager _ownerPM;
    private Rigidbody _handRigidBody;
    private Transform _anchorHandTransform;

    private Vector3 _initPos;
    private Vector3 _collisionPosition;
    private Quaternion _collisionOrientation;
    private Quaternion _initOrient;
    private GameObject _tubeCylinder;
    private Renderer _plierRenderer;
    private Color _initColor;

    // State variables
    private bool _propelling = false;
    private bool _retracting = false;
    private bool _attached = false;

    void Start()
    {
        // Initialize instances
        _handRigidBody = this.GetComponent<Rigidbody>();
        _handRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _initPropulsionSpeed = _propulsionSpeed;
        _ownerPM = _owner.GetComponent<PropulsionManager>();
        _handRigidBody.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
        _anchorHandTransform = this.transform.parent;
        _plierRenderer = this.GetComponent<Renderer>();
        _initColor = _plierRenderer.material.color;

        //Initialize position
        //TODO : Adjust this
        /*
        if((int)_anchorSide == 0) {
            this.transform.localRotation = Quaternion.Euler(0, -90, 0);
        } else {
            this.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        */
        this.transform.position += this.transform.forward * _initialOffset;
        _initPos = this.transform.localPosition;
        _initOrient = this.transform.localRotation;

        //Instantiate hand cylinder
        _tubeCylinder = Instantiate(_tubeCylinderPrefab,Vector3.zero, Quaternion.identity);
        _tubeCylinder.transform.parent = this.transform.parent;
        UpdateCylinderPosition();
    }

    void FixedUpdate()
    {
        // If the plier is being propelled => Move forward according to strengh given in propulsionSpeed
        if(_propelling) {
            this.transform.position = this.transform.position + this.transform.forward * Time.deltaTime * _propulsionSpeed;
        }

        // If the plier is being retracted => Move backward toward initial position until it is close enough
        else if(_retracting) {
            this.transform.position = Vector3.MoveTowards(this.transform.position,
                                                          this.transform.parent.transform.position,
                                                          Time.deltaTime * _initPropulsionSpeed);

            if(Vector3.Distance(this.transform.position, _owner.transform.position) < (0.2f + 0.005)) {
                this.transform.localPosition = _initPos;
                _retracting = false;
                _ownerPM.endedHandRetraction((int) _anchorSide);
            }
        }

        // If the plier is attached : Freeze its position
        // TODO: Change this and make the plier not a child of the left hand anchor anymore ?
        else if(_attached) {
            this.transform.position = _collisionPosition;
            this.transform.rotation = _collisionOrientation;
        }

        //Update cylinder position
        UpdateCylinderPosition();
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

        //this.transform.parent = _anchorHandTransform;
    }

    //TODO : distach from handAnchor if needed
    public void OnTriggerEnter(Collider other) {

        // If the collision is with an anchor object AND the player was propelling the hand (TODO: Change this to player decided to grab)
        // => Attach the plier to the object
        if( (other.gameObject.tag != "NotDragable") & (other.gameObject.tag != "Plier") & _propelling) {
            _propelling = false;
            _retracting = false;
            _attached = true;

            if(other.attachedRigidbody.mass>100.0f) {
              _ownerPM.OnAttachedPlierHeavier(this.transform.position, (int)_anchorSide);
              //  this.transform.parent = null;
            } else {
              // Compute center of mass between plier(with object mass) and player with body mass
              //_ownerPM.OnAttachedPlierObject(center_of_mass, (int)_anchorSide);
            }

            // Save collision properties
            _collisionPosition = this.transform.position + Vector3.back*0.05f;
            _collisionOrientation = this.transform.rotation;

            //Change Material
            _plierRenderer.material.SetColor("_Color", Color.red);
        }
        // If the colliding object is another plier: Then ?
        if(other.gameObject.tag == "Plier") {

        }
    }

    public void OnTriggerExit(Collider other) {
        _plierRenderer.material.SetColor("_Color", _initColor);
    }

    // Reset values after finishing dragging player
    //TODO : Reattach to hand anchor if detached
    public void OnDragFinished() {
        StopRetractHand();
        this.transform.localPosition = _initPos;
        this.transform.localRotation = _initOrient;
    }
    /*
    // Ensures the plier do not move the wall.
    // Yet let the plier retract if player decided to cancel the drag.
    //TODO: Should not be needed
    public void FreezePositionOnDrag() {
      if(!_retracting) {
        this.transform.position = (_collisionPosition - this.transform.forward*0.05f);
      }
    }
    */

    public void UpdatePropulsionSpeed(float factor) {
        if(_propelling) this._propulsionSpeed = _initPropulsionSpeed * factor;
    }

    public void ResetPropulsionSpeed() {
        this._propulsionSpeed = _initPropulsionSpeed;
    }

    private void UpdateCylinderPosition() {
        _tubeCylinder.transform.position = (this.transform.parent.transform.position+this.transform.position)/2;
        _tubeCylinder.transform.LookAt(this.transform.position);
        _tubeCylinder.transform.rotation *= Quaternion.Euler(90.0f, 0.0f, 0.0f);
        _tubeCylinder.transform.localScale = new Vector3(0.1f,0.0f,0.1f)
                                           + Vector3.up * Vector3.Distance(this.transform.parent.transform.position,this.transform.position) / 2;
    }
}
