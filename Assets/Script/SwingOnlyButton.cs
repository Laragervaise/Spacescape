using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SwingOnlyButton : MonoBehaviour {
    public GameObject bat;
    public GameObject backButton;
    private Vector3 _lastPosition = Vector3.zero;
    private Vector3 _speed = Vector3.zero;
    
    void FixedUpdate() {
        Vector3 position = bat.transform.position;
        _speed = (position - _lastPosition)/Time.deltaTime;
        _lastPosition = position;
    }
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject == bat) {
            GetComponent<Rigidbody>().velocity = _speed;
        }
        GetComponent<BoxCollider>().isTrigger = true;
        Task.Delay(500).ContinueWith(t => GetComponent<BoxCollider>().isTrigger = false);
    }

    void onTriggerEnter(Collider otherCollider) {
        if (otherCollider.gameObject == backButton) {
            GetComponent<Rigidbody>().isKinematic = true;
            Task.Delay(5000).ContinueWith(t => GetComponent<Rigidbody>().isKinematic = false);
        }
    }
}