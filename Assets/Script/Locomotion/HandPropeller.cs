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
    public float _initialOffset = 0.2f;
    public GameObject _owner;
    public AnchorSideType _anchorSide;
    public GameObject _tubeCylinderPrefab;
    public GameObject _handController;
    public Mesh _grabbingMesh;
    public Mesh _ungrabbingMesh;

    public float _initPropulsionSpeed=3.0f;
    public float _maxSpeed = 3.0f ;
    public float _time_buffer_attach = 0.5f;
    public float _time_buffer_pushed = 1.0f;
    public float _maxPlierDist = 4.0f;
    public float _minDistToAnchor = 0.3f;
    public float _speedFactor = 0.2f;

    //Private instances
    private PropulsionManager _ownerPM;
    private Rigidbody _handRigidBody;
    private Transform _anchorHandTransform;
    private Grabbable _grabbedObject = null;

    private Vector3 _initPos;
    private Vector3 _collisionPosition;
    private Quaternion _collisionOrientation;
    private Quaternion _initOrient;
    private GameObject _tubeCylinder;
    private float _propulsionSpeed;

    // State variables
    private bool _forceRetract = false;
    private bool _attached = false;
    private bool _attached_object = false;
    private bool _grabbing = false;
    private bool _freeze_propulsion = false;
    private float _time_attached;
    private float _time_pushed;

    //private instances
    private Vector3 _lastPosition;
    private Vector3 _lastPositionLocal;
    private Vector3 _lastPositionOwner;
    private Vector3 _speed;
    private Vector3 _speedLocal;
    private Vector3 _lastPostionAnchor;
    private Quaternion _lastRotAnchor;
    private Vector3 _collisionSpeedEnter = Vector3.zero;

    void Start()
    {
        // Initialize instances
        _handRigidBody = this.GetComponent<Rigidbody>();
        _handRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _handRigidBody.isKinematic = true;
        _handRigidBody.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;

        _propulsionSpeed = 0.0f;
        _ownerPM = _owner.GetComponent<PropulsionManager>();
        _anchorHandTransform = _handController.transform;

        this.transform.position = _anchorHandTransform.position;
        this.transform.rotation = _anchorHandTransform.rotation;
        //this.transform.localRotation *= Quaternion.Euler(90, 90, 0);
        _initPos = this.transform.position - _anchorHandTransform.position;
        _initOrient = this.transform.localRotation;
        _lastPostionAnchor = _anchorHandTransform.position;
        _lastRotAnchor = _anchorHandTransform.rotation;

        //Instantiate hand cylinder
        _tubeCylinder = Instantiate(_tubeCylinderPrefab,Vector3.zero, Quaternion.identity);
        _tubeCylinder.transform.parent = this.transform.parent;

        _lastPosition = transform.position;
        _lastPositionLocal = transform.localPosition;
        _speed = Vector3.zero;
        _time_attached = Time.time;
        UpdateCylinderPosition();

        //Mesh initialization;
        this.GetComponent<MeshFilter>().mesh = _ungrabbingMesh;
    }

    void FixedUpdate()
    {
      //Plier positions
      Vector3 position = transform.position;
      Vector3 positionLocal = transform.localPosition;
      _speed = (position - _lastPosition)/Time.deltaTime;
      _speedLocal = (positionLocal - _lastPositionLocal)/Time.deltaTime;
      _lastPosition = position;
      _lastPositionLocal = positionLocal;
      // If the plier is attached : Freeze its position
        if(_attached) {
            this.transform.position = _collisionPosition;
            this.transform.rotation = _collisionOrientation;
        }

        // If the plier is being propelled => Move forward according to strengh given in propulsionSpeed
        // If retracted => Retract until close enough
        else {

            // Update pliers position with respect to handAnchor
            Quaternion angle = _anchorHandTransform.rotation*Quaternion.Inverse(_lastRotAnchor);
            if(angle != new Quaternion(0.0f,0.0f,0.0f,1.0f)){
              //Not completely working ... ?
              this.transform.RotateAround(_anchorHandTransform.position, Vector3.up, angle.eulerAngles.y);
              this.transform.RotateAround(_anchorHandTransform.position, Vector3.right, angle.eulerAngles.x);
              this.transform.RotateAround(_anchorHandTransform.position, Vector3.forward, angle.eulerAngles.z);
            }

            this.transform.position += (_anchorHandTransform.position - _lastPostionAnchor);

            // Propulsion
            if(_propulsionSpeed>0.1f) {
                if(Vector3.Distance(this.transform.position, _owner.transform.position)<_maxPlierDist) {
                    this.transform.position = this.transform.position + this.transform.forward * Time.deltaTime * _propulsionSpeed;
                }
            }

            //Retraction
            else if(_propulsionSpeed<-0.1f || _forceRetract) {
                if(_forceRetract) this.transform.position = Vector3.MoveTowards(this.transform.position, _anchorHandTransform.position, _initPropulsionSpeed*Time.deltaTime);
                else this.transform.position = Vector3.MoveTowards(this.transform.position, _anchorHandTransform.position, -_propulsionSpeed * Time.deltaTime);
                if(Vector3.Distance(this.transform.position, _anchorHandTransform.position) < _minDistToAnchor) {
                    this.transform.position = _anchorHandTransform.position + _initPos;
                    this.transform.rotation = _anchorHandTransform.rotation;
                    _forceRetract = false;
                    StopRetractHand();
                    _ownerPM.endedForceRetraction((int) _anchorSide);
                }
            }

            // Ensure does not enter in Objects
            if((_collisionSpeedEnter != Vector3.zero) & (!_forceRetract) & (!_attached_object) & (!_attached)) {
                Vector3 new_displacement = (this.transform.position - _lastPosition);
                if(Vector3.Dot(_collisionSpeedEnter, new_displacement) > 0) {
                    this.transform.position = _lastPosition;
                }
            }
        }

        //Update cylinder position if not attached
        UpdateCylinderPosition();
        _lastPostionAnchor = _anchorHandTransform.position;
        _lastRotAnchor = _anchorHandTransform.rotation;
    }

    //////////////
    //
    // ACTIONS
    //
    //////////////
    public void PropulseHand(float factor) {
        if(!_freeze_propulsion) {
            UpdatePropulsionSpeed(factor);
            _forceRetract = false;
        }
    }

    public void StopPropulseHand() {
        ResetPropulsionSpeed();
    }

    public void RetractHand(float factor) {
        if(factor>0) factor = -factor;
        UpdatePropulsionSpeed(factor);
    }

    public void ForceRetractHand() {
        _attached = false;
        _attached_object = false;
        _time_attached = Time.time;
        UpdatePropulsionSpeed(-1.0f);
        _forceRetract = true;
        OnGrabFinish(_grabbedObject);
        _grabbedObject = null;
    }

    public void StopRetractHand() {
        ResetPropulsionSpeed();
    }

    public void StartGrabbingHand() {
        SetGrabbingMesh();
        _grabbing = true;
    }

    public void StopGrabbingHand() {
        SetUnGrabbingMesh();
        _grabbing = false;
        _attached = false;
        _attached_object = false;
        OnGrabFinish(_grabbedObject);
        _grabbedObject = null;
    }

    public void UpdatePropulsionSpeed(float factor) {
        this._propulsionSpeed = _initPropulsionSpeed * factor;
    }

    public void ResetPropulsionSpeed() {
        this._propulsionSpeed = 0.0f;
    }

    private void UpdateCylinderPosition() {
        _tubeCylinder.transform.position = (_anchorHandTransform.transform.position+this.transform.position)/2;
        _tubeCylinder.transform.LookAt(this.transform.position);
        _tubeCylinder.transform.rotation *= Quaternion.Euler(90.0f, 0.0f, 0.0f);
        _tubeCylinder.transform.localScale = new Vector3(0.1f,0.01f,0.1f)
                                           + Vector3.up * Vector3.Distance(_anchorHandTransform.transform.position,this.transform.position) / 2;
    }

    ////////////////
    //
    // COLLISIONS
    //
    ////////////////
    public void OnTriggerEnter(Collider other) {

        /*if((other.gameObject.tag != "Plier") & (other.gameObject.tag != "Player") & (!_attached) & (Time.time - _time_pushed > _time_buffer_pushed)) {
            _time_pushed = Time.time;
            float magnitude = _speedLocal.magnitude;
            magnitude = Mathf.Min(magnitude*_speedFactor, _maxSpeed);
            _owner.GetComponent<Rigidbody>().velocity = Vector3.Normalize(_speed-_owner.GetComponent<Rigidbody>().velocity) * (-magnitude);
        }*/

        if((other.gameObject.tag != "Plier") & (other.gameObject.tag != "Player")) {
            //_freeze_propulsion = true;
            ResetPropulsionSpeed();
            _collisionSpeedEnter = _speed;
        }

        // Save collision properties
        _collisionPosition = this.transform.position;
        _collisionOrientation = this.transform.rotation;
    }

    public void OnTriggerStay(Collider other) {
          // If the collision is with an anchor object AND the player was propelling the hand (TODO: Make Anchor Tag)
          // => Attach the plier to the object
          if((other.gameObject.tag != "Plier") & _grabbing & (other.gameObject.tag != "Player") & (!_attached) &(!_attached_object)& (Time.time - _time_attached > _time_buffer_attach)) {
              _time_attached = Time.time;
              bool isHeavy;

              // We assume the grabbed object is much lighter than the player
              // Compute center of mass between plier(with object mass) and player with body mass
              if(other.attachedRigidbody != null ) {
                  _attached = false;
                  _attached_object = true;
                  isHeavy = false;
                  float total_mass = _ownerPM._playerMass + other.attachedRigidbody.mass;
                  Vector3 center_of_mass = _owner.transform.position*_ownerPM._playerMass/total_mass
                                         + this.transform.position*other.attachedRigidbody.mass/total_mass;

                  //Grab object
                  if(other.GetComponent<Grabbable>() != null) {
                      _grabbedObject = other.gameObject.GetComponent<Grabbable>();
                      _grabbedObject.SetKinematic(true);
                      _grabbedObject.OnGrab(this.gameObject);
                      //_grabbedObject.transform.parent = this.transform;
                      _freeze_propulsion = false;
                  }

                  _ownerPM.OnAttachedPlierObject(center_of_mass, (int)_anchorSide, isHeavy);


              } else {
                  //Attached to heavy object
                  ResetPropulsionSpeed();
                  _attached = true;
                  _attached_object = false;
                  isHeavy = true;

                  //Attract Player in direction
                  _ownerPM.OnAttachedPlierObject(this.transform.position, (int)_anchorSide, isHeavy);
              }
        }

        //TODO: Prevent from entering object =>
        // Compute direction of speed when entering.
        // Project new speed on old speed
        // If result is positive => Freeze position

    }

    public void OnTriggerExit(Collider other) {
        _freeze_propulsion = false;
        _collisionSpeedEnter = Vector3.zero;
        /*
        if((other.attachedRigidbody != null) & (other.GetComponent<Grabbable>() != null)) {
            OnGrabFinish(other.GetComponent<Grabbable>());
        }*/
    }


    /////////////
    //
    // DRAGGING
    //
    /////////////

    // Reset values after finishing dragging player
    //TODO : Reattach to hand anchor if detached
    public void OnDragFinished() {
        StopRetractHand();
        _attached = false;
        _attached_object = false;

        this.transform.position = _anchorHandTransform.position + _initPos;
        this.transform.rotation = _anchorHandTransform.rotation;
        OnGrabFinish(_grabbedObject);
        _grabbedObject = null;
    }

    public void OnGrabFinish(Grabbable grabbed) {
      if(grabbed != null) {
          grabbed.SetKinematic(false);
          //grabbed.transform.parent = null;
          grabbed.OnRelease();
          grabbed.GetComponent<Rigidbody>().velocity = _speed;
          grabbed = null;

      }
    }
    /////////////
    //
    // Mesh Change
    //
    /////////////


    private void SetGrabbingMesh() {
        this.GetComponent<MeshFilter>().mesh = _grabbingMesh;
    }

    private void SetUnGrabbingMesh() {
        this.GetComponent<MeshFilter>().mesh = _ungrabbingMesh;
    }

}
