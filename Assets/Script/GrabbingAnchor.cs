using UnityEngine;

public class GrabbingAnchor : MonoBehaviour {

	//============================//
	// Private instance variables //
	//============================//
	private bool graspingState;
	private int handIndex = -1;

	//==================//
	// Simple accessors //
	//==================//
	public bool getGraspingState () { return graspingState; }
	public int getGraspingId () { return handIndex; }


	public void grab ( int handIndex ) {
		// Set the grasping state of this object

		if ( graspingState ) {
			Debug.LogWarning( "This Gameobject is already held !" );
			return;
		}


		this.handIndex = handIndex;	// Store the id of the hand holding the attached Gameobject
		graspingState = true;           // Set the holding state

		Debug.Log( string.Format("{0} took {1}", handIndex, this.name) );
	}


	public void release(){
		// Reset the grasping state of this object

		if ( !graspingState ) {
			Debug.LogWarning( "This Gameobject is not held !" );
			return;
		}

		handIndex = -1;		// Forget the hand holding this Gameobject
		graspingState = false;	// Update the holding state

		Debug.Log( string.Format( "{0} is released", this.name ) );
	}


	//============================================//
	// Store offsets for the animation of objects //
	//============================================//
	private Quaternion rotationOffset;
	private Vector3 translationOffet;
	private Quaternion initialAnchorRotation;

	public void setPositionOffset( Transform aTransform ){
		// Compute the rotation offset between the controller anchor and the object anchor
		rotationOffset = Quaternion.Inverse( aTransform.rotation ) * this.transform.rotation;

		// Compute the translation offset between the controller anchor and the object anchor
		translationOffet = this.transform.position - aTransform.position;

		// Store the initial rotation of the controller used later to reorient the translation offset
		initialAnchorRotation = aTransform.rotation;
	}

	public void updatePosition( Transform aTransform ) {
		// Do not update the position if the object is not attached
		if ( !graspingState ) return;

		// Apply the new rotation with the rotation offset ( * on the right side as Unity uses indirect referential )
		this.transform.rotation = aTransform.rotation * rotationOffset;

		// Apply the new position with the position offset with the offset translation
		// reoriented with the delta angle of the hand between the grabbing action an now.
		this.transform.position = aTransform.position + ( aTransform.rotation * Quaternion.Inverse( initialAnchorRotation ) ) * translationOffet;
	}
}
