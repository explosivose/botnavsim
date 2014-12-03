using UnityEngine;
using System.Collections;

public class Sensor : MonoBehaviour {

	public enum Type {
		Raycast,
		Spherecast
	}

	public Type type;
	public float radius;
	public bool drawDebug;
	public float maxDistance;

	public Vector3? data {
		get {
			if (type == Type.Raycast) return Raycast ();
			else return Spherecast();
		}
	}
	
	Vector3? Raycast() {
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		Vector3? data = null;
		if (Physics.Raycast(ray, out hit, maxDistance)) {
			data = transform.forward * hit.distance;
			if (drawDebug) Debug.DrawRay(transform.position, 
			                             data.Value, Color.red, 0.05f);
			Draw.Instance.Line(transform.position, transform.position + data.Value, Color.red);
		}
		else {
			if (drawDebug) Debug.DrawRay(transform.position, 
			          transform.forward * maxDistance, Color.green, 0.05f);
			Draw.Instance.Line(transform.position, transform.position + transform.forward * maxDistance, Color.green);
		}
		return data;
	}
	
	Vector3? Spherecast() {
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		Vector3? data = null;
		if (Physics.SphereCast(ray, radius, out hit, maxDistance)) {
			data = transform.forward * hit.distance;
			if (drawDebug) Debug.DrawRay(transform.position, 
		                             data.Value, Color.red,0.1f);
		}
		else {
			if (drawDebug) Debug.DrawRay(transform.position, 
			             transform.forward * maxDistance, Color.green, 0.1f);
		}
		return data;
	}
	
}
