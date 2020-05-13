using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRepairElevator : MonoBehaviour {
    /**
     * When all the SwingOnlyButtons are pushed, trigger the elevator to go down
     */
    
    public GameObject[] swingables;
    public ElevatorDownKinematic elevator;
    public PlayAudio clipToPlay;

    private int buttonsLeft;
    // Start is called before the first frame update
    void Start() {
        buttonsLeft = swingables.Length;
        foreach (GameObject swingable in swingables)
        {
            swingable.GetComponent<SwingOnlyButton>().SetWallRepairElevator(this);
        }
    }

    public void AddButtonDone() {
        if (--buttonsLeft == 0) {
            // Call the elevator
            elevator.StartMoving();
            StartCoroutine(clipToPlay.PlayClipAfterDelay(3));
        }
    }
}
