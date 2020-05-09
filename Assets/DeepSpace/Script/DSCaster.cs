using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DSCaster : MonoBehaviour 
{
    [Header("'Water' - one of built in Unity layers:")]
    public string InteractLayer;
    [Space(10)]
    public GameObject Canvas;
    public float RayLength = 1;
    int buttonsMask;
    Material PrevMat = null;
    Color PrevColor;

    void Awake()
    {
        //interactable layer:
        buttonsMask = LayerMask.GetMask(InteractLayer);
        if(Canvas) Canvas.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.TransformPoint(new Vector3(0, 0, RayLength)), out hit, buttonsMask))
        {
            //execute slave
            if (Canvas) Canvas.SetActive(true);
            if (Input.GetMouseButtonUp(0))
            {
                hit.collider.gameObject.GetComponent<DSTrigger>().DSTriggerExecute();
            }

            //color
            Material NewMat = hit.collider.gameObject.GetComponent<Renderer>().materials[0];
            Color NewColor = NewMat.GetColor("_EmissionColor");
            if (PrevMat != NewMat)
            {
                if (PrevMat)
                {
                    PrevMat.SetColor("_EmissionColor", PrevColor);
                }
                PrevMat = NewMat;
                PrevColor = NewColor;
                NewMat.SetColor("_EmissionColor", new Color(0, 1.27f, 5));
            }
        }
        else
        {
            if (Canvas) Canvas.SetActive(false);
            if (PrevMat)
            {
                PrevMat.SetColor("_EmissionColor", PrevColor);
                PrevMat = null;
            }
        }
    }

    void FixedUpdate()
    {

    }
}
