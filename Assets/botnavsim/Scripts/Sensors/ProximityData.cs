using UnityEngine;
using System.Collections;

public struct ProximityData {
	public Vector3 direction;
	public bool obstructed;
	
	public ProximityData(Vector3 direction, bool obstructed) {
		this.direction = direction;
		this.obstructed = obstructed;
	}
	
	public static bool operator <(ProximityData d1, ProximityData d2) {
		return d1.direction.magnitude < d2.direction.magnitude;
	}
	
	public static bool operator >(ProximityData d1, ProximityData d2) {
		return d1.direction.magnitude > d2.direction.magnitude;
	}
}
