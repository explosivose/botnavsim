using UnityEngine;
using System.Collections;



public class Sensor : MonoBehaviour {

	public struct ProximityData {
		public Vector3 direction;
		public bool obstructed;
	}
	
	public enum Type {
		Raycast,
		Spherecast
	}

	public Type type;
	public float radius;
	public bool drawDebug;
	public float maxDistance;

	public ProximityData data {
		get {
			if (type == Type.Raycast) return Raycast ();
			else return Spherecast();
		}
	}
	
	ProximityData Raycast() {
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		ProximityData data = new ProximityData();
		if (Physics.Raycast(ray, out hit, maxDistance)) {
			data.direction = transform.forward * hit.distance;
			data.obstructed = true;
			if (drawDebug) Debug.DrawRay(transform.position, 
			                             data.direction, Color.red, 0.05f);
			Draw.Instance.Line(transform.position, transform.position + data.direction, Color.red);
		}
		else {
			data.direction = transform.forward * maxDistance;
			data.obstructed = false;
			if (drawDebug) Debug.DrawRay(transform.position, 
			          transform.forward * maxDistance, Color.green, 0.05f);
			Draw.Instance.Line(transform.position, transform.position + transform.forward * maxDistance, Color.green);
		}
		return data;
	}
	
	ProximityData Spherecast() {
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		ProximityData data = new ProximityData();
		if (Physics.SphereCast(ray, radius, out hit, maxDistance)) {
			data.direction = transform.forward * hit.distance;
			data.obstructed = true;
			if (drawDebug) Debug.DrawRay(transform.position, 
		                             data.direction, Color.red,0.1f);
		}
		else {
			data.direction = transform.forward * maxDistance;
			data.obstructed = false;
			if (drawDebug) Debug.DrawRay(transform.position, 
			             transform.forward * maxDistance, Color.green, 0.1f);
		}
		return data;
	}
	
}
