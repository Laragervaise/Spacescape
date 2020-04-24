using UnityEngine;

public class GrabbingDetector : MonoBehaviour {

	// Declare the enumeration to restrict the values for the anchorSide ( thus providing a nice list in the unity inspector )
	public enum AnchorSideType : int { LeftAnchor = 0, RightHand = 1 };

	//==========================//
	// Public instance variable //
	// editable directly within //
	// the inspector            //
	//==========================//
	public double grabbingRadius = 0.1;	// Radius to enable the interaction
	public AnchorSideType anchorSide;	// Define if the hand is a left or right hand




	private GrabbingAnchor[] graspingAnchors;	// Will store objects instance of the GrabbingAnchor class
	void Start () {

		//======================================//
		// Check the anchors for the controller //
		// to later update positions of grasped //
		// objects                              //
		//======================================//
		if( anchorSide == AnchorSideType.LeftAnchor ) {
			if ( this.transform.parent.name != "LeftHandAnchor" ) {
				Debug.LogError( "This script in not a direct child of the LeftHandAnchor !" );
				return;
			}
		} else {
			if ( this.transform.parent.name != "RightHandAnchor" ) {
				Debug.LogError( "This script in not a direct child of the RightHandAnchor !" );
				return;
			}
		}


		//=======================================//
		// Retrieve all the graspable objects    //
		// i.e. the ones with the GrabbingAnchor //
		// script attached to the gameobject     //
		//=======================================//
		graspingAnchors = GameObject.FindObjectsOfType<GrabbingAnchor>();
	}


	private bool handIsClosed () {
		// Is this script is attached to a left anchor
		if ( anchorSide == AnchorSideType.LeftAnchor ) return
			OVRInput.Get( OVRInput.Button.Three )                           // Check that the A button is pressed
			&& OVRInput.Get( OVRInput.Button.Four )                         // Check that the B button is pressed
			&& OVRInput.Get( OVRInput.Axis1D.PrimaryHandTrigger ) > 0.5     // Check that the middle finger is pressing
			&& OVRInput.Get( OVRInput.Axis1D.PrimaryIndexTrigger ) > 0.5;   // Check that the index finger is pressing


		// Is this script is attached to a right anchor
		else return
			OVRInput.Get( OVRInput.Button.One )                             // Check that the A button is pressed
			&& OVRInput.Get( OVRInput.Button.Two )                          // Check that the B button is pressed
			&& OVRInput.Get( OVRInput.Axis1D.SecondaryHandTrigger ) > 0.5   // Check that the middle finger is pressing
			&& OVRInput.Get( OVRInput.Axis1D.SecondaryIndexTrigger ) > 0.5; // Check that the index finger is pressing
	}



	// Forward the update method to the following methods

	// N.B. We can also move the object position update directly within the GrabbingAnchor
	// if we store the controller Anchor GameObject to these instances
	void Update () {
		handleAnchorInteractionDetection();
		updateObjectsPositions();
	}






	bool previousGraspingState = false;	// Store the previous grasping state to detect rising / falling edges triggering the interaction
	void handleAnchorInteractionDetection () {
		//===================================================//
		// This method handles the detection of interactions //
		// and attach / detach anchors of graspable objects  //
		//===================================================//

		// Check if there is a change in the grasping state (and edge), do nothing otherwise
		if ( handIsClosed() == previousGraspingState ) return;
		previousGraspingState = !previousGraspingState;



		//================================================//
		// Define the behavior at grasping the controller //
		//================================================//
		if ( previousGraspingState ) {
			Debug.LogWarning( string.Format( "Entering grabbing for {0}", this.transform.parent.name ) );

			// Determine which object available is the closest from the left hand
			int bestIndex = -1;
			double bestDistance = double.MaxValue;
			double tempDistance;

			for ( int i = 0; i < graspingAnchors.Length; i++ ) {
				if ( !graspingAnchors[i].getGraspingState() ){  // Discard object already attached to other hands
					// Compute the distance between the controller anchor and the item
					tempDistance = Vector3.Distance( this.transform.position, graspingAnchors[i].transform.position );

					// Keep in memory the closest object
					if ( tempDistance < bestDistance ){
						bestIndex = i;
						bestDistance = tempDistance;
					}
				}
			}

			// If the best object is in range grab it
			if ( bestIndex != -1 && bestDistance < grabbingRadius ) {
				Debug.LogWarning( string.Format( "{0} grabbed {1}", this.transform.parent.name, graspingAnchors[bestIndex].name ) );

				graspingAnchors[bestIndex].grab( (int) anchorSide );		// Grab this object
				graspingAnchors[bestIndex].setPositionOffset( this.transform );	// Define the offset on the position
			}



		//=====================================================//
		// Define the behavior at the releasing the controller //
		//=====================================================//
		} else {
			Debug.LogWarning( string.Format( "Releasing grabbing for {0}", this.transform.parent.name ) );

			// Determine whether an object is attached to this hand
			for ( int i = 0; i < graspingAnchors.Length; i++ ) if ( (AnchorSideType) graspingAnchors[i].getGraspingId() == anchorSide ) {
				graspingAnchors[i].release();	// Release this object
				return;				// As only one object can be held in the hand there is no need to go further and waste computational resources
			}
		}
	}


	void updateObjectsPositions () {
		//==========================================================//
		// Update all the grabbing anchors attached to this anchors //
		//==========================================================//
		for( int i = 0; i < graspingAnchors.Length; i++ )
			if ( (AnchorSideType) graspingAnchors[i].getGraspingId() == anchorSide )
				graspingAnchors[i].updatePosition( this.transform );	// Update the position of matching object
	}
}
