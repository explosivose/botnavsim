using UnityEngine;
using System.Collections;

public class Lrf : Sensor {

	public float range;
	public float dt;
	public LayerMask raycastLayer;
	public bool rotating;
	public float rotateSpeed;
	
	private bool scanning;
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
	
	void Update(){
		if (rotating && scanning) {
			transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
		}
	}
}
