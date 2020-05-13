using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class HandPropeller : MonoBehaviour {
    /*
        HAND PROPELLER

        Handles mechanics for the each pliers.
        Receives input from the propulsion Manager

       Attach to GameObject : Right Plier, Left Plier
    */

    // Public variables
    public enum AnchorSideType : int {
        LeftHand = 0,
        RightHand = 1
    };

    public float _initialOffset = 0.2f; // Offset with respect to handAnchor
    public GameObject _owner;           // Robot'sbody
    public AnchorSideType _anchorSide;
    public GameObject _tubeCylinderPrefab;  //Cylinder linking hand to plier
    public GameObject _handController;
    public Mesh _grabbingMesh;
    public Mesh _ungrabbingMesh;

    public float _initPropulsionSpeed = 3.0f;
    public float _maxSpeed = 3.0f;
    public float _time_buffer_attach = 0.5f;    //Avoid attaching constantly on grabbing
    public float _time_buffer_pushed = 1.0f;    //Avoid constantly ^pushing against a wall
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
    private bool _freeze_propulsion = false; // Not used as was too buggy. Would require some rework.
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
    private Quaternion _lastRotPlier;
    private Vector3 _collisionSpeedEnter = Vector3.zero;

    void Start() {
        // Initialize instances
        _handRigidBody = this.GetComponent<Rigidbody>();
        _handRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _handRigidBody.isKinematic = true;
        _handRigidBody.constraints =
            RigidbodyConstraints
                .FreezeRotation; // | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;

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
        _tubeCylinder = Instantiate(_tubeCylinderPrefab, Vector3.zero, Quaternion.identity);
        _tubeCylinder.transform.parent = this.transform.parent;

        _lastPosition = transform.position;
        _lastPositionLocal = transform.localPosition;
        _lastRotPlier = this.transform.rotation;
        _speed = Vector3.zero;
        _time_attached = Time.time;
        UpdateCylinderPosition();

        //Mesh initialization;
        this.GetComponent<MeshFilter>().mesh = _ungrabbingMesh;
    }

    void FixedUpdate() {
        //Save previous Plier positions
        Vector3 position = transform.position;
        Vector3 positionLocal = transform.localPosition;
        _speed = (position - _lastPosition) / Time.deltaTime;
        _speedLocal = (positionLocal - _lastPositionLocal) / Time.deltaTime;
        _lastPosition = position;
        _lastPositionLocal = positionLocal;
        _lastRotPlier = this.transform.rotation;
        // If the plier is attached : Freeze its position
        if (_attached) {
            this.transform.position = _collisionPosition;
            this.transform.rotation = _collisionOrientation;
        }

        // If the plier is being propelled => Move forward according to strengh given in propulsionSpeed
        // If retracted => Retract until close enough
        else {
            // Update pliers position with respect to handAnchor
            Quaternion angle = _anchorHandTransform.rotation * Quaternion.Inverse(_lastRotAnchor);
            if (angle != new Quaternion(0.0f, 0.0f, 0.0f, 1.0f)) {

                //Move plier position around hand Anchor according to rotation of the handAnchor
                this.transform.RotateAround(_anchorHandTransform.position, Vector3.up, angle.eulerAngles.y);
                this.transform.RotateAround(_anchorHandTransform.position, Vector3.right, angle.eulerAngles.x);
                this.transform.RotateAround(_anchorHandTransform.position, Vector3.forward, angle.eulerAngles.z);
            }

            this.transform.position += (_anchorHandTransform.position - _lastPostionAnchor);

            // Propulsion
            if (_propulsionSpeed > 0.1f) {
                if (Vector3.Distance(this.transform.position, _owner.transform.position) < _maxPlierDist) {
                    this.transform.position = this.transform.position +
                                              this.transform.forward * Time.deltaTime * _propulsionSpeed;
                }
            }

            //Retraction
            else if (_propulsionSpeed < -0.1f || _forceRetract) {
                if (_forceRetract)
                    this.transform.position = Vector3.MoveTowards(this.transform.position,
                        _anchorHandTransform.position, _initPropulsionSpeed * Time.deltaTime); //Max speed
                else
                    this.transform.position = Vector3.MoveTowards(this.transform.position,
                        _anchorHandTransform.position, -_propulsionSpeed * Time.deltaTime);   //Speed according to input

                // Reset Position if close to handAnchor
                if (Vector3.Distance(this.transform.position, _anchorHandTransform.position) < _minDistToAnchor) {
                    this.transform.position = _anchorHandTransform.position + _initPos;
                    this.transform.rotation = _anchorHandTransform.rotation;
                    _forceRetract = false;
                    StopRetractHand();
                    _ownerPM.endedForceRetraction((int) _anchorSide);
                }
            }

            // Ensure does not enter in Objects: To be improved
            if ((_collisionSpeedEnter != Vector3.zero) & (!_forceRetract) & (!_attached)) {
                Vector3 new_speed = Vector3.Normalize((this.transform.position - _lastPosition)/2);
                this.transform.position = _lastPosition;
                this.transform.rotation = _lastRotPlier;
            }
        }

        //Update cylinder position
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
        if (!_freeze_propulsion) {
            UpdatePropulsionSpeed(factor);
            _forceRetract = false;
        }
    }

    public void StopPropulseHand() {
        ResetPropulsionSpeed();
    }

    public void RetractHand(float factor) {
        if (factor > 0) factor = -factor;
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
        Vector3 distanceFromBase =
            Vector3.up * Vector3.Distance(_anchorHandTransform.transform.position, transform.position) / 2;

        // Hide cylinder if too small to avoid display issues
        _tubeCylinder.GetComponent<Renderer>().enabled = distanceFromBase.magnitude > 0.01f;

        _tubeCylinder.transform.position = (_anchorHandTransform.transform.position + this.transform.position) / 2;
        _tubeCylinder.transform.LookAt(this.transform.position + Vector3.up * 0.01f);
        _tubeCylinder.transform.rotation *= Quaternion.Euler(90.0f, 0.0f, 0.0f);
        _tubeCylinder.transform.localScale = new Vector3(0.06f, 0.01f, 0.06f) + distanceFromBase;
    }

    ////////////////
    //
    // COLLISIONS
    //
    ////////////////
    public void OnTriggerEnter(Collider other) {
        // Code used for pushing against walls but too impractical so was removed, would need more work
        /*if((other.gameObject.tag != "Plier") & (other.gameObject.tag != "Player") & (!_attached) & (Time.time - _time_pushed > _time_buffer_pushed)) {
            _time_pushed = Time.time;
            float magnitude = _speedLocal.magnitude;
            magnitude = Mathf.Min(magnitude*_speedFactor, _maxSpeed);
            _owner.GetComponent<Rigidbody>().velocity = Vector3.Normalize(_speed-_owner.GetComponent<Rigidbody>().velocity) * (-magnitude);
        }*/
        _collisionSpeedEnter = Vector3.zero;
        if ((!other.gameObject.CompareTag("Plier")) & (!other.gameObject.CompareTag("Player")) & (other.GetComponent<Grabbable>() == null)) {
            //_freeze_propulsion = true;
            ResetPropulsionSpeed();
            _collisionSpeedEnter = Vector3.Normalize(_speed);
        }

        // Save collision properties
        _collisionPosition = transform.position;
        _collisionOrientation = transform.rotation;
    }

    public void OnTriggerStay(Collider other) {
        // => Attach the plier to the object
        if ((!other.gameObject.CompareTag("Plier")) & _grabbing & (!other.gameObject.CompareTag("Player")) & (!_attached) &
            (!_attached_object) & (Time.time - _time_attached > _time_buffer_attach)) {
            _time_attached = Time.time;
            bool isHeavy;

            //Grab light object
            if (other.GetComponent<Grabbable>() != null) {
                _attached = false;
                _attached_object = true;
                isHeavy = false;
                _grabbedObject = other.gameObject.GetComponent<Grabbable>();
                _grabbedObject.SetKinematic(true);
                _grabbedObject.OnGrab(this.gameObject);
                _freeze_propulsion = false;

                if (other.attachedRigidbody != null) {
                    // Compute center of mass between plier(with object mass) and player with body mass
                    float total_mass = _ownerPM._playerMass + other.attachedRigidbody.mass;
                    Vector3 center_of_mass = _owner.transform.position * _ownerPM._playerMass / total_mass
                                             + this.transform.position * other.attachedRigidbody.mass / total_mass;
                    _ownerPM.OnAttachedPlierObject(center_of_mass, (int) _anchorSide, isHeavy);
                }
                else {
                    _ownerPM.OnAttachedPlierObject(this.transform.position, (int) _anchorSide, isHeavy);
                }
            }
            else {
                //Attached to heavy object
                ResetPropulsionSpeed();
                _attached = true;
                _attached_object = false;
                isHeavy = true;

                //Attract Player in direction
                _ownerPM.OnAttachedPlierObject(this.transform.position, (int) _anchorSide, isHeavy);
            }
        }
    }

    public void OnTriggerExit(Collider other) {
        _freeze_propulsion = false;
        _collisionSpeedEnter = Vector3.zero;
    }


    /////////////
    //
    // DRAGGING
    //
    /////////////

    // Reset values after finishing dragging player
    public void OnDragFinished() {
        StopRetractHand();
        _attached = false;
        _attached_object = false;

        this.transform.position = _anchorHandTransform.position + _initPos;
        this.transform.rotation = _anchorHandTransform.rotation;
        OnGrabFinish(_grabbedObject);
        _grabbedObject = null;
    }

    // Reset obejct state when releasing it
    public void OnGrabFinish(Grabbable grabbed) {
        if (grabbed != null) {
            grabbed.SetKinematic(false);
            grabbed.OnRelease();
            Vector3 speed = _speed;
            Task.Delay(30).ContinueWith(t => grabbed.GetComponent<Rigidbody>().velocity = speed);
            grabbed.GetComponent<Rigidbody>().velocity = speed;
        }
    }
    /////////////
    //
    // Mesh Change On Grab
    //
    /////////////


    private void SetGrabbingMesh() {
        this.GetComponent<MeshFilter>().mesh = _grabbingMesh;
    }

    private void SetUnGrabbingMesh() {
        this.GetComponent<MeshFilter>().mesh = _ungrabbingMesh;
    }
}
