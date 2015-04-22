using UnityEngine;
using System.Collections;

/// <summary>
/// Environment data
/// </summary>
public class Environment : MonoBehaviour {

	/// <summary>
	/// Transform in hierarchy detailing bounds for origin by collider.bounds
	/// </summary>
	public Transform origin;
	
	/// <summary>
	/// Transform in hierarchy detailing bounds for destination by collider.bounds
	/// </summary>
	public Transform destination;
	
	/// <summary>
	/// Gets the origin bounds.
	/// </summary>
	/// <value>The origin bounds.</value>
	public Bounds originBounds {
		get {
			return origin.collider.bounds;
		}
	} 
	
	/// <summary>
	/// Gets the destination bounds.
	/// </summary>
	/// <value>The destination bounds.</value>
	public Bounds destinationBounds {
		get {
			return destination.collider.bounds;
		}
	}
	
}
