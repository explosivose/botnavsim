using UnityEngine;
using System.Collections;

/// <summary>
/// A test script that tracks trains a camera on a target object.
/// This was used to test and build robot models. 
/// </summary>
public class TestCamera : MonoBehaviour {

	public Transform target;	// object to look at, set in the unity editor
	
	// Update is called once per frame
	void Update () {
		if (target) transform.LookAt(target);
	}
}
