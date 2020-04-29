using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSTrigger : MonoBehaviour 
{
    public bool IsActive = true;
    public GameObject Executor;
    public enum ExecMode
    {
        elevator_up = 0,
        elevator_dwn = 1,
        gate = 2,
        elevator_call = 3,
    }
    public ExecMode ExecModes = ExecMode.gate;

	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    public void DSTriggerExecute()
    {
        if (IsActive)
        {
            if (ExecModes == ExecMode.elevator_up)
            {
                Executor.GetComponent<DSElevator>().StartMoving(true);
            }
            else if (ExecModes == ExecMode.elevator_dwn)
            {
                Executor.GetComponent<DSElevator>().StartMoving(false);
            }
            else if (ExecModes == ExecMode.gate)
            {
                Executor.GetComponent<DSGates>().StartOpenClose();
            }
            else if (ExecModes == ExecMode.elevator_call)
            {
                Executor.GetComponent<DSElevatorCaller>().StartMoving();
            }
        }
    }
}
