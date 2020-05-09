using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNewClass : MonoBehaviour {

	/* ... */

	void Start() { /* ... */ }

	void Update() {
		if( OVRInput.Get( OVRInput.Button.One ) ) Debug.LogWarning( "Button is pressed" );
	}

	/* ... */
}
