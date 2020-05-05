using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerGetter : MonoBehaviour
{
    // This class is a workaround as the device does not handle properly GetDown and GetUp inputs.

    //Defines types for this class
    public enum AnchorSideType : int {
        LeftHand = 0,
        RightHand = 1
    };

    //private instances
    private bool _wasExtending = false;
    private bool _wasRetracting = false;
    private bool _wasCancelling = false;

    public GameObject _plier;
    private HandPropeller _plierPropeller;

    //Allows to switch between controllers or keyboard
    public bool controllers = true;
    public AnchorSideType _anchorSide;

    void Start() {
        _plierPropeller = _plier.GetComponent<HandPropeller>();
    }

    void FixedUpdate()
    {
        // Update Inputs
        OVRInput.Update();

        // Update _propulsionSpeed
        if(controllers) {
            float factor;
            if(_anchorSide == AnchorSideType.LeftHand) {
                factor = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            } else {
                factor = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
            }
            this._plierPropeller.UpdatePropulsionSpeed(factor);
        }
    }


    ///////////////////////
    //
    //    Retrieving Controls
    //
    //////////////////////
    public bool getExtensionStart() {
      bool extention_input;
      float factor = 1.0f;

      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) {
              factor = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
              extention_input = factor>0.1f;
          } else {
              factor = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
              extention_input = factor>0.1f;
          }
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) extention_input = Input.GetKey("a");
          else                                       extention_input = Input.GetKey("p");
      }
      //Returns true the first frame it is being pressed
      if((!_wasExtending) & (extention_input)) {
          this._plierPropeller.UpdatePropulsionSpeed(factor);
          _wasExtending = true;
          return true;
      }
      return false;
    }


    public bool getExtensionStop() {
      bool extention_input;

      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) extention_input = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.1f;
          else                                       extention_input = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.1f;
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) extention_input = Input.GetKey("a");
          else                                       extention_input = Input.GetKey("p");
      }

      // Returns true the first frame it is being released
      if((_wasExtending) & (!extention_input)) {
          this._plierPropeller.ResetPropulsionSpeed();
          _wasExtending = false;
          return true;
      }
      return false;
    }

    public bool getRetractionStart() {
      bool retraction_input;
      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) retraction_input = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0.1f;
          else                                       retraction_input = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger)>0.1f;
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) retraction_input = Input.GetKey("z");
          else                                       retraction_input = Input.GetKey("o");
      }

      //Returns true the first frame it is being pressed
      if((!_wasRetracting) & (retraction_input)) {
          _wasRetracting = true;
          return true;
      }
      return false;
    }

    public bool getRetractionStop() {
      bool retraction_input;
      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) retraction_input = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0.1f;
          else                                       retraction_input = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger)>0.1f;
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) retraction_input = Input.GetKey("z");
          else                                       retraction_input = Input.GetKey("o");
      }
      //Returns true the first frame it is being released
      if((_wasRetracting) & (!retraction_input)) {
          _wasRetracting = false;
          return true;
      }
      return false;
    }

    public bool getCancel() {
      bool cancel_input;
      if(controllers) {
          if(_anchorSide == AnchorSideType.LeftHand) cancel_input = OVRInput.Get(OVRInput.Button.Three);
          else                                       cancel_input = OVRInput.Get(OVRInput.Button.One);
      } else {
          if(_anchorSide == AnchorSideType.LeftHand) cancel_input = Input.GetKey("e");
          else                                       cancel_input = Input.GetKey("i");
      }

      //Returns true the first frame it is being pressed
      return cancel_input;
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
