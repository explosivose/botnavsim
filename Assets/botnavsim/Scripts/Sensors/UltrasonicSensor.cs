using UnityEngine;
using System.Collections;

public class UltrasonicSensor : Sensor {
	
	public float updateDt;
	public float Fov;
	public float maxRange;
	public bool test;
	public LayerMask raycastLayer;
	
	private float _lastUpdateTime;
	private ProximityData _data;
	private Color _recieveColor;
	private Color _hitColor;
	private Color _missColor;

	public override ProximityData GetData () {
		if (Time.time > _lastUpdateTime + updateDt) Scan();
		return _data;
	}
	
	// scan for obstructions in a cone 
	private void Scan() {
		_lastUpdateTime = Time.time;
		float proximity = maxRange;
		// roll
		for (float r = 0; r <= 180f; r += 10f) {
			// yaw
			for (float y = -Fov; y < Fov; y += 2f){
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
	}
	
	// cast a ray and return detected obstruction if the hit angle is less than a threshold
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
	
	void Awake() {
		_recieveColor = Color.red;
		_recieveColor.a = 0.5f;
		_hitColor = Color.green;
		_hitColor.a = 0.25f;
		_missColor = Color.grey;
		_missColor.a = 0.5f;
	}
	
	void Update() {
		if (test) {
			GetData();
		}
	}
	
	// draw sensor ray approximation in Unity Editor
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
