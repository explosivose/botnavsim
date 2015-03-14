using UnityEngine;
using System.Collections;

// param sensor returns the sensor direction at the smallest
// range inside a cone 

public class ParamSensor : MonoBehaviour, ISensor {

	public bool drawDebug = true;
	public float FOV = 20f;
	public float maxRange = 20f;
	public float maxUpdateRate = 5f;
	
	
	private float 			_lastUpdateTime;
	private ProximityData 	_data;
	
	
	public ProximityData GetData () {
		if (Time.time > _lastUpdateTime + 1f/maxUpdateRate) {
			UpdateData();
		}
		return _data;
	}
	
	
	private void UpdateData() {
		_lastUpdateTime = Time.time;
		float proximity = maxRange;
		for (float a = -FOV; a < FOV; a += 2f) {
			Quaternion rotation = Quaternion.Euler(new Vector3(0,a,0));
			Vector3 direction = rotation * transform.forward;
			float check = Cast (direction);
			if (check < proximity) proximity = check;
		}
		_data.direction = transform.forward * proximity;
		if (proximity < maxRange * 0.75f) _data.obstructed = true;
		else _data.obstructed = false;
	}
	
	private float Cast(Vector3 direction) {
		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, maxRange)) {
			if (drawDebug) Draw.Instance.Line(
				transform.position, 
				hit.point,
				Color.red);
			return hit.distance;
		}
		else  {
			if (drawDebug) Draw.Instance.Line(
				transform.position, 
				transform.position + direction * maxRange,
				Color.green);
			return maxRange;
		}
	}
}
