using System.Threading.Tasks;
using UnityEngine;

public class SwingOnlyButton : MonoBehaviour {
    public GameObject bat;
    public GameObject backButton;
    public GameObject cylinder;
    private Vector3 _lastPosition = Vector3.zero;
    private Vector3 _speed = Vector3.zero;

    void FixedUpdate() {
        Vector3 position = bat.transform.position;
        _speed = (position - _lastPosition) / Time.deltaTime;
        _lastPosition = position;
        UpdateCylinderPosition();
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject == bat) {
            GetComponent<Rigidbody>().velocity = _speed;
        }

        GetComponent<BoxCollider>().isTrigger = true;
        Task.Delay(500).ContinueWith(t => GetComponent<BoxCollider>().isTrigger = false);
    }

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject == backButton) {
            transform.position = backButton.transform.position;
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void UpdateCylinderPosition() {
        var position = backButton.transform.position;
        cylinder.transform.position = (position + transform.position) / 2;
        cylinder.transform.localScale = new Vector3(0.2f, 0.0f, 0.2f)
                                        + Vector3.up * Vector3.Distance(position, transform.position) / 2;
    }
}