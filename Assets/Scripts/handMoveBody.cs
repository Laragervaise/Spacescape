using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handMoveBody : MonoBehaviour {
    public GameObject handBody;
    
    // Start is called before the first frame update
    void Start() {
        handBody.transform.position = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        handBody.transform.position = this.transform.position;
    }
}
