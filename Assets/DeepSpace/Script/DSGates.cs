using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSGates : MonoBehaviour 
{
    public string AnimationOpen;
    public string AnimationClose;
    public enum StartStates
    {
        opened = 0,
        closed = 1,
    }
    public StartStates StartState = StartStates.closed;
    bool CurrentState;

	// Use this for initialization
	void Start () 
    {
        if (StartState == StartStates.closed)
        {
            GetComponent<Animation>().Play(AnimationClose);
            CurrentState = true;
        }
        else
        {
            GetComponent<Animation>().Play(AnimationOpen);
            CurrentState = false;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    public void StartOpenClose()
    {
        Debug.Log("doooors");
        Debug.Log(GetComponent<Animation>().isPlaying.ToString());
        if (!GetComponent<Animation>().isPlaying)
        {
            CurrentState = !CurrentState;
        }
        else
        {
            return;
        }

        if (CurrentState)
        {
            if (!GetComponent<Animation>().isPlaying)
            {
                GetComponent<Animation>().Play(AnimationClose);
            }
        }
        else
        {
            if (!GetComponent<Animation>().isPlaying)
            {
                GetComponent<Animation>().Play(AnimationOpen);
            }
        }
    }
}
