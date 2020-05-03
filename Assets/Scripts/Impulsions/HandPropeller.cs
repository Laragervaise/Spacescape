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
    public float _initPropulsionSpeed=3.0f;
    public float _maxSpeed = 3.0f ;
    public float _time_buffer_attach = 0.5f;
    public float _maxPlierDist = 4.0f;
    public float _minDistToAnchor = 0.3f;
    public float _speedFactor = 0.5f;



    //Private instances
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
    private float _propulsionSpeed;

    // State variables
    private bool _forceRetract = false;
    private bool _attached = false;
    private bool _grabbing = false;
    private float _time_attached;




    //private instances
    private Vector3 _lastPosition;
    private Vector3 _lastPositionLocal;
    private Vector3 _lastPositionOwner;
    private Vector3 _speed;
    private Vector3 _speedLocal;

    void Start()
    {
        // Initialize instances
        _handRigidBody = this.GetComponent<Rigidbody>();
        _handRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        _handRigidBody.isKinematic = false;
        _propulsionSpeed = 0.0f;
        _ownerPM = _owner.GetComponent<PropulsionManager>();
        _handRigidBody.constraints = RigidbodyConstraints.FreezeRotation;// | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
        _anchorHandTransform = this.transform.parent;
        _plierRenderer = this.GetComponent<Renderer>();
        _initColor = _plierRenderer.material.color;

        //Initialize position
        //TODO : COMMENT THIS ON VR , USED FOR COMPUTER TESTING
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

        _lastPosition = transform.position;
        _lastPositionLocal = transform.localPosition;
        _lastPositionOwner = _owner.transform.position;
        _speed = Vector3.zero;
        _time_attached = Time.time;
        UpdateCylinderPosition();
    }

    void FixedUpdate()
    {
      //Camera position
      _lastPositionOwner = _owner.transform.position;

      //Plier positions
      Vector3 position = transform.position;
      Vector3 positionLocal = transform.localPosition;
      _speed = (position - _lastPosition)/Time.deltaTime;
      _speedLocal = (positionLocal - _lastPositionLocal)/Time.deltaTime;
      _lastPosition = position;
      _lastPositionLocal = positionLocal;

      //Make kinematic when close to player to avoid several bugs
      /*if(Vector3.Distance(_owner.GetComponent<CapsuleCollider>().ClosestPoint(this.transform.position), this.transform.position)<0.5) {
          _handRigidBody.isKinematic = true;
      } else {
          _handRigidBody.isKinematic = false;
      }*/

      // If the plier is attached : Freeze its position
      // TODO: Change this and make the plier not a child of the left hand anchor anymore ?
        if(_attached) {
            this.transform.position = _collisionPosition;
            this.transform.rotation = _collisionOrientation;
        }
        // If the plier is being propelled => Move forward according to strengh given in propulsionSpeed
        // If retracted => Retract until close enough

        else if(_propulsionSpeed>0.1f) {
            if(Vector3.Distance(this.transform.position, _owner.transform.position)<_maxPlierDist) {
                this.transform.position = this.transform.position + this.transform.forward * Time.deltaTime * _propulsionSpeed;
            }
        }
        else if(_propulsionSpeed<-0.1f || _forceRetract) {
            if(_forceRetract) this.transform.position = Vector3.MoveTowards(this.transform.position, _anchorHandTransform.position, _initPropulsionSpeed*Time.deltaTime);
            else this.transform.position = Vector3.MoveTowards(this.transform.position, _anchorHandTransform.position, -_propulsionSpeed * Time.deltaTime);
            if(Vector3.Distance(this.transform.position, _anchorHandTransform.position) < _minDistToAnchor) {
                this.transform.localPosition = _initPos;
                this.transform.localRotation = _initOrient;
                _forceRetract = false;
                StopRetractHand();
                _ownerPM.endedForceRetraction((int) _anchorSide);
            }
        }
        else {
            //Update position if needed
            //TODO: FIX THIS !!! :(
            _handRigidBody.MovePosition(_handRigidBody.position +  _ownerPM.GetTranslation());
        }

        //Update cylinder position if not attached
        UpdateCylinderPosition();
    }

    //////////////
    //
    // ACTIONS
    //
    //////////////
    public void PropulseHand(float factor) {
        UpdatePropulsionSpeed(factor);
        _forceRetract = false;
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
        UpdatePropulsionSpeed(-1.0f);
        _forceRetract = true;
    }

    public void StopRetractHand() {
        ResetPropulsionSpeed();
    }

    public void StartGrabbingHand() {
        _grabbing = true;
    }

    public void StopGrabbingHand() {
        _grabbing = false;
    }


    ////////////////
    //
    // COLLISIONS
    //
    ////////////////
    public void OnCollisionEnter(Collision collision) {
        if((collision.gameObject.tag != "Plier") & (collision.gameObject.tag != "Player") & (!_attached)) {
            Vector3 normal = collision.contacts[0].normal;
            //Vector3 normal = -this.transform.forward;
            float magnitude = Vector3.Project(_speedLocal + _owner.GetComponent<Rigidbody>().velocity, normal).magnitude;
            magnitude = Mathf.Min(magnitude*_speedFactor, _maxSpeed);
            _owner.GetComponent<Rigidbody>().velocity = normal * (magnitude);
        }
    }

    public void OnCollisionStay(Collision other) {
        // If the collision is with an anchor object AND the player was propelling the hand (TODO: Make Anchor Tag)
        // => Attach the plier to the object
        if((other.gameObject.tag != "Plier") & _grabbing & (other.gameObject.tag != "Player") & (!_attached) & (Time.time - _time_attached > _time_buffer_attach)) {

            ResetPropulsionSpeed();
            _attached = true;
            _time_attached = Time.time;

            _ownerPM.OnAttachedPlierHeavier(this.transform.position, (int)_anchorSide);
              //CancelParent();

              // Compute center of mass between plier(with object mass) and player with body mass
              //float total_mass = _ownerPM._playerMass + other.attachedRigidbody.mass;
              //Vector3 center_of_mass = _owner.transform.position*_ownerPM._playerMass/total_mass
              //                       + this.transform.position*other.attachedRigidbody.mass/total_mass;

              //_ownerPM.OnAttachedPlierObject(center_of_mass, (int)_anchorSide);



            // Save collision properties
            _collisionPosition = this.transform.position;
            _collisionOrientation = this.transform.rotation;

            //Change Material
            _plierRenderer.material.SetColor("_Color", Color.white);
        }
        // If the colliding object is another plier: Then ?
        if(other.gameObject.tag == "Plier") {
        }
    }

    public void OnCollisionExit(Collision other) {
        _plierRenderer.material.SetColor("_Color", _initColor);
    }


    /////////////
    //
    // DRAGGING
    //
    /////////////

    // Reset values after finishing dragging player
    //TODO : Reattach to hand anchor if detached
    public void OnDragFinished() {
        //ResetParent();
        StopRetractHand();
        _attached = false;
        this.transform.localPosition = _initPos;
        this.transform.localRotation = _initOrient;
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
        _tubeCylinder.transform.localScale = new Vector3(0.1f,0.0f,0.1f)
                                           + Vector3.up * Vector3.Distance(_anchorHandTransform.transform.position,this.transform.position) / 2;
    }

    public void CancelParent() {
        this.transform.parent = null;
    }

    public void ResetParent() {
        this.transform.parent = _anchorHandTransform;
    }
}
