using UnityEngine;
using System.Collections;

public class Sensor : MonoBehaviour {

	public float maxDistance;

	public Vector3? data {
		get {
			Ray ray = new Ray(transform.position, transform.forward);
			RaycastHit hit;
			Vector3? data = null;
			if (Physics.Raycast(ray, out hit, maxDistance)) {
				data = transform.forward * hit.distance;
			}
			return data;
		}
	}
}
