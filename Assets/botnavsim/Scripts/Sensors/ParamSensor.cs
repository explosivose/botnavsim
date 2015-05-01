using UnityEngine;
using System.Collections;

// param sensor returns the sensor direction at the smallest
// range inside a cone 

/// <summary>
/// Parameter sensor is a 2D FOV sensor model, loosely based on real 
/// ultrasonic sensor behaviour
/// </summary>
public class ParamSensor : Sensor {

	// public fields
	// ~-~-~-~-~-~-


	/// <summary>
	/// If true the sensor will use Draw to draw raycast lines in the simulation
	/// </summary>
	public bool drawDebug = true;
	
	/// <summary>
	/// The field of view angle in degrees.
	/// </summary>
	public float FOV = 20f;
	
	/// <summary>
	/// The maximum distance for each raycast.
	/// </summary>
	public float maxRange = 20f;
	
	/// <summary>
	/// The sensor update delta time in seconds.
	/// </summary>
	public float updateDt = 0.2f;
	
	
	// public properties
	// -~-~-~-~-~-~-~-~
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="ParamSensor"/> is scanning.
	/// </summary>
	/// <value><c>true</c> if scanning; otherwise, <c>false</c>.</value>
	public bool scanning {
		get; private set;
	}
	
	// private members
	// ~-~-~-~-~-~-~-
	
	/// <summary>
	/// The Robot.SensorData callback to pass ProximityData to
	/// </summary>
	private Robot.SensorData _callback;
	
	/// <summary>
	/// Cached ProximityData from the last UpdateData() scan
	/// </summary>
	private ProximityData 	_data;
	
	
	// public functions
	// ~-~-~-~-~-~-~-

	// sensor class override
	public override void Enable (Robot.SensorData callback) {
		scanning = true;
		_callback = callback;
		StartCoroutine(UpdateData());
	}

	// sensor class override
	public override void Disable () {
		scanning = false;
	}
	
	// private methods
	// -~-~-~-~-~-~-~-~
	
	/// <summary>
	/// Update data coroutine. Once started, executes until disabled. 
	/// Calls <see cref="_callback"/> on <see cref="Robot"/> every update.
	/// Update rate defined by <see cref="updateDt"/>
	/// </summary>
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
	
	/// <summary>
	/// Wrapper function for a single raycast. Returns the proximity distance
	/// in the specified direction. 
	/// </summary>
	/// <param name="direction">Direction.</param>
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
