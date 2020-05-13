using UnityEngine;

public class Grabbable : MonoBehaviour {
    /*

          GRABBABLE

          Attached to any object we want to be able to grab.
          On grab, save the pliers position and fixe the object position and Orientation to the one of the grabbing plier;
          On release, set back object properties.

          If not attached, the player will retract towards this object on dragging.

          Attach To GameObject : Any object we want to grab.

     */

    //public instances
    public Vector3 _eulerAngleOffset = Vector3.zero;
    public Vector3 _offsetPosition = Vector3.zero;
    public Vector3 _offsetPositionOnPlier = new Vector3(0, 0, 0.5f);

    //private instances
    [HideInInspector]
    public bool _is_grabbed = false;
    private GameObject _grabAnchor = null;

    // Udpates position (with a small offset if needed) and orientation according to anchor position and rotation
    void FixedUpdate() {
        if ((_is_grabbed) & (_grabAnchor != null)) {
            this.transform.position = _grabAnchor.transform.position +
                                      _grabAnchor.transform.rotation * (_offsetPosition + _offsetPositionOnPlier);
            this.transform.rotation = _grabAnchor.transform.rotation * Quaternion.Euler(_eulerAngleOffset);
        }
    }

    public void OnGrab(GameObject anchor) {
        if (!_is_grabbed) {
            _is_grabbed = true;
            _grabAnchor = anchor;
        }
    }

    public void OnRelease() {
        if (_is_grabbed) {
            _is_grabbed = false;
            _grabAnchor = null;
        }
    }

    public void SetKinematic(bool isKinematic) {
        if (this.GetComponent<Rigidbody>() != null) this.GetComponent<Rigidbody>().isKinematic = isKinematic;
    }
}
