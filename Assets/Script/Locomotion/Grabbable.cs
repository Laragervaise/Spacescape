using UnityEngine;

public class Grabbable : MonoBehaviour {
    //public instances
    public Vector3 _eulerAngleOffset = Vector3.zero;
    public Vector3 _offsetPosition = Vector3.zero;
    public Vector3 _offsetPositionOnPlier = new Vector3(0, 0, 0.8f);

    //private instances
    private bool _is_grabbed = false;
    private GameObject _grabAnchor = null;

    // Update is called once per frame
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