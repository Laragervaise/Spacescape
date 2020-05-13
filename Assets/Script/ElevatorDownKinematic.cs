using UnityEngine;

public class ElevatorDownKinematic : MonoBehaviour {
    /*
     * Modified the code from the asset pack DeepSpace to meet our need
     * Can call the elevator down only, and set it kinematic to avoid pushing the player
     * Set it back to not kinematic afterwards.
     */
    
    
    [Header("Current mesh position, e.g. 1,2,3,4.. floor (from 1)")]
    public int StartFloor;

    [Space(10)] public int TotalFloors;
    [Header("smaller = faster")] public float Speed = 5f;
    public MeshCollider collider;

    [HideInInspector] public bool IsMoving;
    [HideInInspector] public bool CallerBusy;
    [HideInInspector] public int CurrentFloor;
    float TimeStart;
    Vector3 StartPos;
    Vector3 EndPos;


    // Use this for initialization
    void Start() {
        CurrentFloor = StartFloor;
    }

    // Update is called once per frame
    void Update() {
        if (IsMoving) {
            float STimeStart = Time.time - TimeStart;
            float Percent = STimeStart / Speed;
            transform.position = Vector3.Lerp(StartPos, EndPos, Percent);
            if (Percent >= 1f) {
                IsMoving = false;
                collider.enabled = true;
            }
        }
    }

    public void StartMoving() {
        if (!IsMoving & !CallerBusy) {
            if ((CurrentFloor - 1) >= 1) CurrentFloor--;
            else return;

            IsMoving = true;
            collider.enabled = false;
            TimeStart = Time.time;
            StartPos = transform.position;
            EndPos = transform.position - Vector3.up * 4;
        }
    }
}