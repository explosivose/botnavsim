using UnityEngine;
using System.Collections;

/// <summary>
/// Sensor model for the MS Kinect IR depth sensors
/// </summary>
public class KinectIR : Sensor {

	public float dt = 0.033f;
	public LayerMask raycastLayer;
	
	private float horizontalFov = 58f;
	private float verticalFov = 45.1f;
	private float maxRange = 3.5f;
	
	private bool scanning;
	private Robot.SensorData callback;
	
	public override void Enable (Robot.SensorData callback)
	{
		this.callback = callback;
		StartCoroutine(Scan());
	}

	public override void Disable ()
	{
		scanning = false;
	}
	
	private IEnumerator Scan() {
		scanning = true;
		while(scanning) {
			for(float h = -horizontalFov/2f; h < horizontalFov/2f; h+=1f) {
				for(float v = -verticalFov/2f; v < verticalFov/2f; v+=1f) {
					Quaternion rotation;
					Vector3 direction;
					
					rotation = Quaternion.AngleAxis(h, transform.up);
					direction = rotation * transform.forward;
					rotation = Quaternion.AngleAxis(v, transform.right);
					direction = rotation * direction;
					Ray ray = new Ray(transform.position, direction);
					RaycastHit hit;
					float proximity = maxRange;
					bool obstructed = false;
					if (Physics.Raycast(ray, out hit, maxRange, raycastLayer)) {
						proximity = hit.distance;
						obstructed = true;
						Draw.Instance.Line(transform.position, hit.point, Color.red);
					}
					callback(new ProximityData(direction * proximity, obstructed));
				}
			}
			yield return new WaitForSeconds(dt);
		}
	}
	
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.grey;
		for(float h = -horizontalFov/2f; h < horizontalFov/2f; h+=10f) {
			for(float v = -verticalFov/2f; v < verticalFov/2f; v+=10f) {
				Quaternion rotation;
				Vector3 direction;
				
				rotation = Quaternion.AngleAxis(h, transform.up);
				direction = rotation * transform.forward;
				rotation = Quaternion.AngleAxis(v, transform.right);
				direction = rotation * direction;
				
				Gizmos.DrawRay(transform.position, direction * maxRange);
			}
		}
	}
}
