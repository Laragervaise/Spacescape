using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSElevatorCaller : MonoBehaviour
{
    [Header("!caller doesn't work with Y pos < 0 (negative numbers)!")]
    public DSElevator _DSElevator;
    [Header("smaller = faster")]
    public float Speed = 5f;

    bool IsMoving;
    float TimeStart;
    Vector3 StartPos;
    Vector3 EndPos;

    // Use this for initialization
    void Start()
    {
        StartPos = Vector3.zero;
        EndPos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            float STimeStart = Time.time - TimeStart;
            float Percent = STimeStart / Speed;
            _DSElevator.transform.position = new Vector3(_DSElevator.transform.position.x, Mathf.Lerp(StartPos.y, EndPos.y, Percent), _DSElevator.transform.position.z);
            if (Percent >= 1f)
            {
                IsMoving = false;
                _DSElevator.CallerBusy = false;
            }
        }
    }

    public void StartMoving()
    {
        if (!IsMoving & !_DSElevator.IsMoving)
        {

            StartPos.y = _DSElevator.transform.position.y;
            EndPos.y = (int)transform.position.y;
            EndPos.y = (int)EndPos.y / 4;
            EndPos.y = EndPos.y * 4;

            if (EndPos.y == StartPos.y)
            {
                return;
            }

            IsMoving = true;
            _DSElevator.CallerBusy = true;
            TimeStart = Time.time;

            int floors = 0;
            if(StartPos.y < 0 | EndPos.y < 0)
            {
                floors = (int)(Mathf.Abs(StartPos.y) + Mathf.Abs(EndPos.y));
            }
            else
            {
                floors = (int)Mathf.Abs(StartPos.y - EndPos.y);
            }
            floors = (int)floors / 4;

            if (!(StartPos.y == EndPos.y))
            {
                if (StartPos.y > EndPos.y)
                {
                    _DSElevator.CurrentFloor = _DSElevator.CurrentFloor - floors;
                }
                else
                {
                    _DSElevator.CurrentFloor = _DSElevator.CurrentFloor + floors;
                }
            }
        }
    }

}
