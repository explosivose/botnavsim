using UnityEngine;
using System.Collections;

public class Sensor : MonoBehaviour {

	public bool drawDebug;
	public float maxDistance;

	public Vector3? data {
		get {
			Ray ray = new Ray(transform.position, transform.forward);
			RaycastHit hit;
			Vector3? data = null;
			if (Physics.Raycast(ray, out hit, maxDistance)) {
				data = transform.forward * hit.distance;
				if (drawDebug) Debug.DrawRay(transform.position, 
					data.Value, Color.green);
			}
			else {
				if (drawDebug) Debug.DrawRay(transform.position, 
					transform.forward * maxDistance, Color.red);
			}
			return data;
		}
	}
}
