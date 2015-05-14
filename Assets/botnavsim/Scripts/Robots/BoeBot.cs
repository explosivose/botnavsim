using UnityEngine;
using System.Collections;

/// <summary>
/// BoeBot differential drive control system.
/// Built using data from: https://www.parallax.com/product/900-00008
/// See final report for diagram of implementation
/// </summary>
[RequireComponent(typeof(Robot))]
public class BoeBot : MonoBehaviour {

	// public fields
	
	/// <summary>
	/// The max wheel RPM (50RPM for parallax continuous rotation servo)
	/// </summary>
	public float maxWheelRpm = 50f;
	
	/// <summary>
	/// The max wheel torque (0.268NM for parallax continuous rotation servo).
	/// </summary>
	public float maxWheelTorque = 0.268f;
	
	/// <summary>
	/// Control system gain parameter.
	/// </summary>
	public float Kp = 0.5f;
	
	// private members
	// -~-~-~-~-~-~-~
	
	// References to robot topology
	private Robot 			_robot;
	private WheelCollider 	_leftWheel;
	private WheelCollider 	_rightWheel;
	
	private float forwardAngle;
	private float frontBack;
	private float rightAngle;
	private float rightLeft;
	
	private float leftTarget;
	private float rightTarget;
	
	private float leftTorque;
	private float rightTorque;
	// called when robot is instantiated
	void Awake() {
		_robot = GetComponent<Robot>();
		_leftWheel = transform.Find("Left Wheel").GetComponent<WheelCollider>();
		_rightWheel = transform.Find("Right Wheel").GetComponent<WheelCollider>();
	}
	
	void Update() {

		if (!_robot.moveEnabled) {
			_leftWheel.motorTorque = 0f;
			_rightWheel.motorTorque = 0f;
			_leftWheel.brakeTorque = 10f;
			_rightWheel.brakeTorque = 10f;
			return;	
		} 
		
		_leftWheel.brakeTorque = 0f;
		_rightWheel.brakeTorque = 0f;
		
		
		Vector3 target = _robot.navigationCommand;
		target.y = 0f;
		target.Normalize();
		Debug.DrawRay(transform.position, target, Color.green);
		
		if(target.magnitude < 0.1f) {
			rightTarget = 0f;
			leftTarget = 0f;
		} else {
			forwardAngle = Vector3.Angle(target, transform.forward);
			frontBack = Mathf.Cos(forwardAngle * Mathf.Deg2Rad);		//1:forward, -1:backward, 0:(left or right)
			
			rightAngle = Vector3.Angle(target, transform.right);
			rightLeft = Mathf.Cos(rightAngle * Mathf.Deg2Rad);		//1:right, -1:left, 0:(forward or back)
			
			rightTarget = Mathf.Clamp(frontBack+rightLeft, -1f, 1f);
			leftTarget = Mathf.Clamp(frontBack-rightLeft, -1f, 1f);
		}
		
		// drive wheel
		leftTorque = (leftTarget*maxWheelRpm - _leftWheel.rpm) * Kp;
		_leftWheel.motorTorque = Mathf.Clamp(leftTorque, -maxWheelTorque, maxWheelTorque);
		rightTorque = (rightTarget*maxWheelRpm - _rightWheel.rpm) * Kp;
		_rightWheel.motorTorque = Mathf.Clamp(rightTorque, -maxWheelTorque, maxWheelTorque);
		
		Debug.DrawRay(_leftWheel.transform.position, transform.forward * leftTarget * 0.1f, Color.magenta);
		Debug.DrawRay(_rightWheel.transform.position, transform.forward * rightTarget * 0.1f, Color.magenta);

	}
	
	
}
