using UnityEngine;
using System.Collections;

// param sensor returns the sensor direction at the smallest
// range inside a cone 

public class ParamSensor : Sensor {

	public bool drawDebug = true;
	public float FOV = 20f;
	public float maxRange = 20f;
	public float updateDt = 0.2f;
	
	private Robot.SensorData _callback;
	private ProximityData 	_data;
	
	public override void Enable (Robot.SensorData callback) {
		scanning = true;
		_callback = callback;
		StartCoroutine(UpdateData());
	}

	public override void Disable () {
		scanning = false;
	}

	public bool scanning {
		get; private set;
	}
	
	private IEnumerator UpdateData() {
		
		while(scanning) {
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
			_callback(_data);
			yield return new WaitForSeconds(updateDt);
		}
	
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
