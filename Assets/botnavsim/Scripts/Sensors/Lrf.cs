using UnityEngine;
using System.Collections;

/// <summary>
/// Laser Range Finder (LRF) sensor model
/// </summary>
public class Lrf : Sensor {

	/// <summary>
	/// The maximum range.
	/// </summary>
	public float range;
	
	/// <summary>
	/// Delta time between measurements.
	/// </summary>
	public float dt;
	
	/// <summary>
	/// The raycast collision layer.
	/// </summary>
	public LayerMask raycastLayer;
	
	/// <summary>
	/// If <c>true</c> this sensor object will rotate 
	/// </summary>
	public bool rotating;
	
	/// <summary>
	/// Speed of rotation in degrees per second
	/// </summary>
	public float rotateSpeed;
	
	/// <summary>
	/// Boolean flag indicating whether this sensor is scanning.
	/// </summary>
	private bool scanning;
	
	/// <summary>
	/// The callback function for the robot when new proximity data is ready
	/// </summary>
	private Robot.SensorData callback;
	
	public override void Enable (Robot.SensorData callback)
	{
		this.callback = callback;
		if (!scanning)
			StartCoroutine(Scan());	
	}

	public override void Disable ()
	{
		scanning = false;
	}
	
	/// <summary>
	/// Scan for proximity
	/// </summary>
	private IEnumerator Scan() {
		scanning = true;
		while(scanning) {
			Ray ray = new Ray(transform.position, transform.forward * range);
			RaycastHit hit;
			float proximity = range;
			bool obstructed = false;
			if (Physics.Raycast(ray, out hit, range, raycastLayer)) {
				proximity = hit.distance;
				obstructed = true;
				Draw.Instance.Line(transform.position, hit.point, Color.red);
			} else {
				Draw.Instance.Bearing(transform.position, transform.forward * range, Color.grey);
			}
			callback(new ProximityData(proximity * transform.forward, obstructed));
			yield return new WaitForSeconds(dt);
		}
	}
	
	/// <summary>
	/// Update this instance. (rotate if rotating is <c>true</c>)
	/// </summary>
	void Update(){
		if (rotating && scanning) {
			transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
		}
	}
}
