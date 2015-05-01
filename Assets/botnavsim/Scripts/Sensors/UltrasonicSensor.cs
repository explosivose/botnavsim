using UnityEngine;
using System.Collections;

/// <summary>
/// Ultrasonic sensor model. 
/// </summary>
public class UltrasonicSensor : Sensor {
	
	// public fields
	// ~-~-~-~-~-~-
	
	/// <summary>
	/// The sensor update delta time in seconds.
	/// </summary>
	public float updateDt;
	
	/// <summary>
	/// The field of view angle in degrees.
	/// </summary>
	public float Fov;
	
	/// <summary>
	/// The max range of the sensor in metres.
	/// </summary>
	public float maxRange;
	
	/// <summary>
	/// The raycast collision layer. 
	/// </summary>
	public LayerMask raycastLayer;
	
	
	// private members
	// -~-~-~-~-~-~-~-~
	
	/// <summary>
	/// The callback function to execute when the sensor has new data to share.
	/// </summary>
	private Robot.SensorData _callback;
	
	/// <summary>
	/// Cached proximity data from the last scan
	/// </summary>
	private ProximityData _data;
	
	
	private Color _recieveColor;
	private Color _hitColor;
	private Color _missColor;
	
	
	public override void Enable (Robot.SensorData callback) {
		scanning = true;
		_callback = callback;
		StartCoroutine(Scan());
	}

	public override void Disable () {
		scanning = false;
	}

	public bool scanning {
		get; private set;
	}
	
	/// <summary>
	/// Scan for obstructions in a cone
	/// </summary>
	private IEnumerator Scan() {
		
		while(scanning) {
			float proximity = maxRange;
			// roll
			for (float r = 0; r <= 180f; r += 10f) {
				// yaw
				for (float y = -Fov; y < Fov; y += 6f){
					Quaternion rotation;
					Vector3 direction;
					rotation = Quaternion.AngleAxis(y, transform.up);
					direction = rotation * transform.forward;
					rotation = Quaternion.AngleAxis(r, transform.forward);
					direction = rotation * direction;
					float cast = Cast(direction);
					// if this cast is shorter than returnData
					if (cast < proximity) proximity = cast;
				}
			}
			_data = new ProximityData(transform.forward * proximity, proximity < maxRange);
			_callback(_data);
			yield return new WaitForSeconds(updateDt);
		}
		
	}
	
	/// <summary>
	/// Cast a ray and return detected obstruction if the hit angle is less than a threshold
	/// </summary>
	/// <param name="direction">Direction.</param>
	private float Cast(Vector3 direction) {
		Ray ray = new Ray(transform.position, direction);
		RaycastHit hit;
		float proximity = maxRange;
		if (Physics.Raycast(ray, out hit, maxRange, raycastLayer)) {
			// if the hit angle is small enough, then assume the sound was reflected and recieved
			if (Vector3.Angle(-hit.normal, direction) < 30f) {
				proximity = hit.distance;
				Draw.Instance.Line(
					transform.position, 
					hit.point,
					_recieveColor);
				Draw.Instance.Line(
					hit.point,
					hit.point+hit.normal*0.1f,
					Color.magenta);
			}
			else {
				Draw.Instance.Line(
					transform.position, 
					hit.point,
					_hitColor);
				Draw.Instance.Line(
					hit.point,
					hit.point+hit.normal*0.1f,
					Color.cyan);
			}
		}
		else {
			Draw.Instance.Line(
				transform.position, 
				transform.position + direction * maxRange,
				_missColor);
		}
		
		return proximity;
	}
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_recieveColor = Color.red;
		_recieveColor.a = 0.5f;
		_hitColor = Color.green;
		_hitColor.a = 0.25f;
		_missColor = Color.grey;
		_missColor.a = 0.5f;
	}
	
	
	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos() {
		Gizmos.color = Color.grey;
		// roll
		for (float r = 0; r <= 180f; r += 30f) {
			// yaw
			for (float y = -Fov; y <= Fov; y += 10f){
				Quaternion rotation = Quaternion.AngleAxis(y, transform.up);
				Vector3 direction = rotation * transform.forward;
				rotation = Quaternion.AngleAxis(r, transform.forward);
				direction = rotation * direction;
				Gizmos.DrawRay(transform.position, direction * maxRange);
			}
		}
	}
}
