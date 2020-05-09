using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    //public instances


    //private instances
    private bool _is_grabbed = false;
    private GameObject _grabAnchor = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if( (_is_grabbed)  & (_grabAnchor!=null) ) {
            this.transform.position = _grabAnchor.transform.position;
        }
    }

    public void OnGrab(GameObject anchor){
        if(!_is_grabbed) {
            _is_grabbed = true;
            _grabAnchor = anchor;
        }
    }

    public void OnRelease() {
         if(_is_grabbed) {
            _is_grabbed = false;
            _grabAnchor = null;
         }
    }

    public void SetKinematic(bool boolean) {
        if(this.GetComponent<Rigidbody>() != null)    this.GetComponent<Rigidbody>().isKinematic = boolean;
    }
}
