using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class BoeBot : MonoBehaviour {

	// public fields
	public float torque;
	public float window = 0.1f;
	
	// References to robot topology
	private Robot 			_robot;
	private WheelCollider 	_leftWheel;
	private WheelCollider 	_rightWheel;
	
	
	// PID control variables
	private Vector3 		_moveDirection;
	private float 			_integral;
	private float 			_previousAngle;
	
	void Awake() {
		_robot = GetComponent<Robot>();
		_leftWheel = transform.Find("Left Wheel").GetComponent<WheelCollider>();
		_rightWheel = transform.Find("Right Wheel").GetComponent<WheelCollider>();
		
	}
	
	void Update() {
		Vector3 right = transform.right;
		_moveDirection = _robot.moveCommand;
		
		// Ignore y component
		right.y = 0f;
		_moveDirection.y = 0f;
		
		// positive 1 if need to turn right, negative 1 if need to turn left
		float error = Vector3.Dot(right, _moveDirection);
		if (error > window) {
			// turn right
			_rightWheel.motorTorque = torque;
			_leftWheel.motorTorque = -torque;
		}
		else if (error < -window) {
			// turn left
			_rightWheel.motorTorque = -torque;
			_leftWheel.motorTorque = torque;
		}
		else {
			// go forwards
			_rightWheel.motorTorque = torque;
			_leftWheel.motorTorque = torque;
		}
		
		

	}
}
