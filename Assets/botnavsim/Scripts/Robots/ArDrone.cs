using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class ArDrone : MonoBehaviour {
	
	public float forwardThrust = 2f;
	public Pid verticalController;
	public Propeller[] propellers;
	public float targetHeight = 4f;
	public Pid angularThrustController;
	
	
	private Robot _robot;
	
	
	void Awake() {
		_robot = GetComponent<Robot>();
	}
	
	void Update() {
		foreach(Propeller propeller in propellers) {
			propeller.controller.CopySettings(verticalController);
		}
	}
	
	void FixedUpdate() {
		if (!_robot.moveEnabled) return;
		
		foreach(Propeller propeller in propellers) {
			float height = MeasureHeight(propeller.tr.position);
			float thrust = propeller.controller.output(targetHeight, height);
			thrust = Mathf.Clamp(thrust, 0, 10f);
			if (thrust > 0) {
				rigidbody.AddForceAtPosition(Vector3.up * thrust, propeller.tr.position);
				Debug.DrawRay(propeller.tr.position, Vector3.down * thrust, Color.magenta);
			}
		}
		
		
		Vector3 yaw = -transform.right;
		yaw.y = 0;
		Debug.DrawRay(transform.position, yaw * 2f, Color.red);
		Vector3 targetYaw = Vector3.right;
		targetYaw.y = 0f;
		Debug.DrawRay(transform.position, targetYaw * 3f, Color.green);
		
		float bearing = Vector3.Dot(yaw, targetYaw);
		float ang = angularThrustController.output(0f, bearing);
		rigidbody.AddTorque(0f, ang, 0f);
		

		rigidbody.AddForce(_robot.navigationCommand.normalized * 2f);
		
	}
	
	float MeasureHeight(Vector3 localPosition) {
		Ray ray = new Ray(localPosition, Vector3.down);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return hit.distance;
		}
		else {
			return 0f;
		}
	}
	
	void OnDrawGizmos() {
		Vector3 size = Vector3.one * 5f;
		size.y = 2f;
		Gizmos.color = Color.Lerp(Color.black, Color.clear, 0.5f);
		Gizmos.DrawCube(transform.position, size);
	}
}
