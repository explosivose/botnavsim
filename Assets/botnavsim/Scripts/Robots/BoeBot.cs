using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class BoeBot : MonoBehaviour {

	// public fields
	public float maxWheelRpm = 50f;
	[Header("Warning: Kp = 1 will crash Unity")]
	public float Kp = 0.5f;
	
	
	
	// References to robot topology
	private Robot 			_robot;
	private WheelCollider 	_leftWheel;
	private WheelCollider 	_rightWheel;
	
	private float forwardangle;
	private float frontback;
	private float rightangle;
	private float rightleft;
	
	private float leftTarget;
	private float rightTarget;
	
	// called when robot is instantiated
	void Awake() {
		_robot = GetComponent<Robot>();
		_leftWheel = transform.Find("Left Wheel").GetComponent<WheelCollider>();
		_rightWheel = transform.Find("Right Wheel").GetComponent<WheelCollider>();
	}
	
	void Update() {

		if (!_robot.moveEnabled) return;
		
		Vector3 target = _robot.navigationCommand.normalized;
		Debug.DrawRay(transform.position, target, Color.green);
		
		if(target.magnitude < 0.1f) {
			rightTarget = 0f;
			leftTarget = 0f;
		} else {
			forwardangle = Vector3.Angle(target, transform.forward);
			frontback = Mathf.Cos(forwardangle * Mathf.Deg2Rad);		//1:forward, -1:backward, 0:(left or right)
			
			rightangle = Vector3.Angle(target, transform.right);
			rightleft = Mathf.Cos(rightangle * Mathf.Deg2Rad);		//1:right, -1:left, 0:(forward or back)
			
			rightTarget = Mathf.Clamp(frontback+rightleft, -1f, 1f);
			leftTarget = Mathf.Clamp(frontback-rightleft, -1f, 1f);
		}
		
		// drive wheel
		_leftWheel.motorTorque = (leftTarget*maxWheelRpm - _leftWheel.rpm) * Kp;
		_rightWheel.motorTorque = (rightTarget*maxWheelRpm - _rightWheel.rpm) * Kp;
		
		Debug.DrawRay(_leftWheel.transform.position, transform.forward * leftTarget * 0.1f, Color.magenta);
		Debug.DrawRay(_rightWheel.transform.position, transform.forward * rightTarget * 0.1f, Color.magenta);

	}
	
	
}
