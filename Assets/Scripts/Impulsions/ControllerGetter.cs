using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerGetter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();
    }

    public bool getExtensionStart() {
      return OVRInput.GetDown(OVRInput.Button.Two);
    }

    public bool getExtensionStop() {
      return OVRInput.GetUp(OVRInput.Button.Two);
    }

    public bool getRetractionStart() {
      return OVRInput.GetDown(OVRInput.Button.One);
    }

    public bool getRetractionStop() {
      return OVRInput.GetUp(OVRInput.Button.One);
    }

    public bool getCancel() {
      return false;
    }

}
