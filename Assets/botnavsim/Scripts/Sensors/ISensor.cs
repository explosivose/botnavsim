using UnityEngine;
using System.Collections;

public struct ProximityData {
	public Vector3 direction;
	public bool obstructed;
}

public interface ISensor {

	ProximityData GetData();
}
