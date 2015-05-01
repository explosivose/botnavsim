using UnityEngine;
using System.Collections;

/// <summary>
/// Proximity data structure. 
/// </summary>
public struct ProximityData {
	/// <summary>
	/// The direction of the proximity
	/// </summary>
	public Vector3 direction;
	
	/// <summary>
	/// Indicates whether or not the location at the end of the direction is obstructed.
	/// </summary>
	public bool obstructed;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ProximityData"/> struct.
	/// </summary>
	/// <param name="direction">Direction.</param>
	/// <param name="obstructed">If set to <c>true</c> obstructed.</param>
	public ProximityData(Vector3 direction, bool obstructed) {
		this.direction = direction;
		this.obstructed = obstructed;
	}
	
	/// <summary>Compares direction.magnitudes</summary>
	/// <param name="d1">D1.</param>
	/// <param name="d2">D2.</param>
	public static bool operator <(ProximityData d1, ProximityData d2) {
		return d1.direction.magnitude < d2.direction.magnitude;
	}
	
	/// <summary>Compares direction.magnitudes</summary>
	/// <param name="d1">D1.</param>
	/// <param name="d2">D2.</param>
	public static bool operator >(ProximityData d1, ProximityData d2) {
		return d1.direction.magnitude > d2.direction.magnitude;
	}
}
