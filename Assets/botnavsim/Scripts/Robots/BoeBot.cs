using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class BoeBot : MonoBehaviour {

	// public fields
	public float maxTorque;
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
		_moveDirection = _robot.navigationCommand;
		
		// do nothing if move command is small (expected length is 1f)
		if (_moveDirection.magnitude < 0.5f) {
			_rightWheel.motorTorque = 0f;
			_leftWheel.motorTorque = 0f;
			_rightWheel.brakeTorque = maxTorque;
			_leftWheel.brakeTorque = maxTorque;
			return;
		}
		
		// do nothing if robot move disabled
		if (!_robot.moveEnabled) {
			_rightWheel.motorTorque = 0f;
			_leftWheel.motorTorque = 0f;
			_rightWheel.brakeTorque = maxTorque;
			_leftWheel.brakeTorque = maxTorque;
			return;
		}
		
		// Ignore y component
		right.y = 0f;
		_moveDirection.y = 0f;
		
		// positive 1 if need to turn right, negative 1 if need to turn left
		float error = Vector3.Dot(right, _moveDirection);
		if (error < window) {
			// turn right
			_rightWheel.motorTorque = 0;
			_leftWheel.motorTorque = maxTorque;
			_rightWheel.brakeTorque = maxTorque * Mathf.Abs(error);
			_leftWheel.brakeTorque = maxTorque/3f;
		}
		else if (error > -window) {
			// turn left
			_rightWheel.motorTorque = maxTorque;
			_leftWheel.motorTorque = 0;
			_rightWheel.brakeTorque = maxTorque/3f;
			_leftWheel.brakeTorque = maxTorque * Mathf.Abs(error);
		}
		else {
			// go forwards
			_rightWheel.motorTorque = maxTorque;
			_leftWheel.motorTorque = maxTorque;
			_rightWheel.brakeTorque = 0;
			_leftWheel.brakeTorque = 0;
		}
		
		

	}
}
