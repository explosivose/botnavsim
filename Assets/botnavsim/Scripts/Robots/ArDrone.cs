using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class ArDrone : MonoBehaviour {

	[System.Serializable]
	public class Pid {
		public float kp, ki, kd;
		public float error {get; private set;}
		public float previous_error {get; private set;}
		public float integral {get; private set;}
		public float derivative {get; private set;}
		
		public float output(float target, float actual) {
			error = target - actual;
			integral += error * Time.fixedDeltaTime;
			derivative = (error - previous_error) / Time.fixedDeltaTime;
			previous_error = error;
			return (kp*error) + (ki*integral) + (kd*derivative);
		}
		
		public void CopySettings(Pid pid) {
			kp = pid.kp;
			ki = pid.ki;
			kd = pid.kd;
		}
	}
	
	[System.Serializable]
	public class Propeller {
		public Transform tr;
		public Pid controller;
		
	}
	
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
		Vector3 targetYaw = _robot.moveCommand;
		targetYaw.y = 0f;
		Debug.DrawRay(transform.position, targetYaw * 3f, Color.green);
		
		float bearing = Vector3.Dot(yaw, targetYaw);
		float ang = angularThrustController.output(0f, bearing);
		rigidbody.AddTorque(0f, ang, 0f);
		
		if (Mathf.Abs(bearing) < 0.25f) {
			rigidbody.AddForce(_robot.moveCommand.normalized * 2f);
		}
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
}
