using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartWithDelay : MonoBehaviour
{
    public float delay;
    void Start()
     {
         Invoke("ResetScene", delay);
     }

     void ResetScene()
     {
         Application.LoadLevel(Application.loadedLevel);
     }
}
