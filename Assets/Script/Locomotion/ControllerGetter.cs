using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerGetter : MonoBehaviour
{
    /*
        CONTROLLER GETTER

        Provides an interface to retrieve inputs from the attached controller and set vibrations on attach.
        Functions are called by propulsionManager at each fixedUpdate

        Attach to  GameObject: LeftController, Right Controller
    */

    //Defines types for this class
    public enum AnchorSideType : int {
        LeftHand = 0,
        RightHand = 1
    };

    //Public instances
    public GameObject _plier;
    public bool controllers = true;     //Allows to switch between controllers or keyboard for debugging
    public AnchorSideType _anchorSide;

    //private instances
    private bool _wasExtending = false;
    private bool _wasRetracting = false;
    private bool _wasCancelling = false;
    private bool _wasGrabbing = false;
    private HandPropeller _plierPropeller;



    void Start() {
        _plierPropeller = _plier.GetComponent<HandPropeller>();
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        // Update Inputs
        OVRInput.Update();
    }


    ///////////////////////
    //
    //    Retrieving Controls
    //
    //////////////////////
    public float getPlierExtensionSpeed() {
      float factor = 0.0f;

      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) {
              factor = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
          } else {
              factor = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
          }
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) {
            if(Input.GetKey("a")) factor = 1.0f;
            if(Input.GetKey("z")) factor = -1.0f;
          } else {
            if(Input.GetKey("p")) factor = 1.0f;
            if(Input.GetKey("o")) factor = -1.0f;
          }
      }

      return factor;
    }

    public bool getCancel() {
      bool cancel_input;
      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) cancel_input = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
          else                                       cancel_input = OVRInput.Get(OVRInput.Button.SecondaryThumbstick);
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) cancel_input = Input.GetKey("e");
          else                                       cancel_input = Input.GetKey("i");
      }

      return cancel_input;
    }

    public bool getGrabbingStart() {
        bool grabbing_input;
        if(controllers) {
            if(_anchorSide == AnchorSideType.LeftHand) grabbing_input = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0.1f;
            else                                       grabbing_input = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger)>0.1f;
        } else {
            if(_anchorSide == AnchorSideType.LeftHand) grabbing_input = Input.GetKey("d");
            else                                       grabbing_input = Input.GetKey("k");
        }

        //Returns true the first frame it is being pressed
        if((!_wasGrabbing) & (grabbing_input)) {
            _wasGrabbing = true;
            return true;
        }
        return false;
    }

    public bool getGrabbingStop() {
        bool grabbing_input;
        if(controllers) {
            if(_anchorSide == AnchorSideType.LeftHand) grabbing_input = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0.1f;
            else                                       grabbing_input = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger)>0.1f;
        } else {
            if(_anchorSide == AnchorSideType.LeftHand) grabbing_input = Input.GetKey("d");
            else                                       grabbing_input = Input.GetKey("k");
        }

        //Returns true the first frame it is being pressed
        if((_wasGrabbing) & (!grabbing_input)) {
            _wasGrabbing = false;
            return true;
        }
        return false;
    }


    ///////////////////////
    //
    //    Vibration
    //
    //////////////////////
    public void SetControllerVibrationOn(float duration) {
        if(controllers) StartCoroutine("Vibrate", duration);
    }

    IEnumerator Vibrate(float duration) {
      if(_anchorSide == AnchorSideType.LeftHand)   OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.LTouch);
      else                                        OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.RTouch);

      yield return new WaitForSeconds(duration);
      if(_anchorSide == AnchorSideType.LeftHand)   OVRInput.SetControllerVibration(0,0, OVRInput.Controller.LTouch);
      else                                        OVRInput.SetControllerVibration(0,0, OVRInput.Controller.RTouch);
    }
}
