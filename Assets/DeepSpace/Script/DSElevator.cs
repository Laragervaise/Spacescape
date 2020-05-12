using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSElevator : MonoBehaviour 
{
    [Header("Current mesh position, e.g. 1,2,3,4.. floor (from 1)")]
    public int StartFloor;
    [Space(10)]
    public int TotalFloors;
    [Header("smaller = faster")]
    public float Speed = 5f;

    [HideInInspector]
    public bool IsMoving;
    [HideInInspector]
    public bool CallerBusy;
    [HideInInspector]
    public int CurrentFloor;
    float TimeStart;
    Vector3 StartPos;
    Vector3 EndPos;


	// Use this for initialization
	void Start () 
    {
        CurrentFloor = StartFloor;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (IsMoving)
        {
            float STimeStart = Time.time - TimeStart;
            float Percent = STimeStart / Speed;
            transform.position = Vector3.Lerp(StartPos, EndPos, Percent);
            if (Percent >= 1f)
            {
                IsMoving = false;
            }
        }
	}

    public void StartMoving(bool direction)
    {
        if (!IsMoving & !CallerBusy)
        {
            if (direction)
            {
                if ((CurrentFloor + 1) <= TotalFloors) CurrentFloor++;
                else return;
            }
            else
            {
                if ((CurrentFloor - 1) >= 1) CurrentFloor--;
                else return;
            }
            IsMoving = true;
            TimeStart = Time.time;
            StartPos = transform.position;
            if (direction) EndPos = transform.position + Vector3.up * 4;
            else EndPos = transform.position - Vector3.up * 4;
        }
    }
}
