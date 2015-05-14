using UnityEngine;
using System.Collections;

/// <summary>
/// Sensor model for the MS Kinect IR depth sensors. 
/// This sensor model is very bandwidth intensive, calling
/// the Robot.SensorData callback function 2400 times per 
/// Kinect IR frame (determined by dt).
/// </summary>
public class KinectIR : Sensor {

	/// <summary>
	/// Delta time between Kinect IR frames (15Hz or 30Hz)
	/// </summary>
	public float dt = 0.033f;
	
	/// <summary>
	/// The raycast collision layer.
	/// </summary>
	public LayerMask raycastLayer;
	
	/// <summary>
	/// The horizontal field of view (from datasheet).
	/// </summary>
	private float horizontalFov = 58f;
	
	/// <summary>
	/// The vertical field of view (from data sheet).
	/// </summary>
	private float verticalFov = 45.1f;
	
	/// <summary>
	/// The max range in meters.
	/// </summary>
	private float maxRange = 3.5f;
	
	/// <summary>
	/// Boolean indicates whether this instance is scanning.
	/// </summary>
	private bool scanning;
	
	/// <summary>
	/// The callback function for the robot.
	/// </summary>
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
	
	/// <summary>
	/// Scan routine. 
	/// </summary>
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
	
	
	/// <summary>
	/// Raises the draw gizmos event ( indicates field of view in Unity Editor viewport)
	/// </summary>
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
