using System.Threading.Tasks;
using UnityEngine;

public class SwingOnlyButton : MonoBehaviour {
    /**
     * Make this button/object only swingable by a specific object (bat)
     * When it reaches the position of "backButton", is sticks there and tell its
     * parentTrigger that he was triggered.
     *
     * Only the speed of the bat at the moment of the impact matters, it is transmitted to
     * the button and the button loses his collider for a short while. The button must be
     * linked to the "backButton" by a joint spring!
     */
    
    public GameObject bat;
    public GameObject backButton;
    public GameObject cylinder;
    private WallRepairElevator parentTrigger;
    private bool doneTrigger = false;
    private Vector3 _lastPosition = Vector3.zero;
    private Vector3 _speed = Vector3.zero;

    public void SetWallRepairElevator(WallRepairElevator parent) {
        parentTrigger = parent;
    }

    /**
     * Get the speed of the bat, as it is held by the player, computing its speed must be done
     * manually. Update the cylinder that visually links this button to "backButton"'s position
     */
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

    /**
     * Check for collision with "backButton" and apply the various effects
     */
    public void OnTriggerEnter(Collider other) {
        if (!doneTrigger && other.gameObject == backButton) {
            transform.position = backButton.transform.position;
            GetComponent<Rigidbody>().isKinematic = true;
            doneTrigger = true;
            if (parentTrigger != null) {
                parentTrigger.AddButtonDone();
            }
        }
    }

    /**
     * Update the visual cylinders linking the button to "backButton"'s position
     */
    private void UpdateCylinderPosition() {
        var position = backButton.transform.position;
        cylinder.transform.position = (position + transform.position) / 2;
        cylinder.transform.localScale = new Vector3(0.2f, 0.0f, 0.2f)
                                        + Vector3.up * Vector3.Distance(position, transform.position) / 2;
    }
}