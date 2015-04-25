using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class ArDrone : MonoBehaviour {
	
	public float maxTilt = 25f;
	public float tiltKp;
	public float maxYawVelocity = 0.5f;
	public float yawKp;
	public Pid thrustContoller;
	
	
	public Transform FL;
	public Transform FR;
	public Transform BL;
	public Transform BR;
	
	
	
	private Robot _robot;
	
	private Vector3 _hold;			// maintain this forward direction in yaw control when there is no navigation command
	
	private float fLeftTilt;
	private float fLeftAngleError;
	private float fRightTilt;
	private float fRightAngleError;
	
	private Vector3 _controlTorque;			// yaw on transform.up axis
	private Vector3 _flControlThrust;		// front left propeller force (transform.down axis)
	private Vector3 _frControlThrust;		// front right propeller force
	private Vector3 _blControlThrust;		// back left propeller force
	private Vector3 _brControlThrust;		// back right propeller force
	
	void Awake() {
		_robot = GetComponent<Robot>();
	}
	
	void Start() {
		_hold = transform.forward;
	}
	
	// calculate control forces
	void Update() {
		
		if (!_robot.moveEnabled) return;
		
		Vector3 command = _robot.navigationCommand.normalized;
			
		Vector3 right2d;	// transform.right with no y component
		Vector3 target2d;	// target direction with no y component
		
		// if no command
		if (command.magnitude < 0.1f) {		// no command, replace command vector with hold values
			// calculate yaw control to face command direction
			right2d = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
			target2d = new Vector3(_hold.x, 0f, _hold.z);
			
		} else {	// deal with command vector
			// calculate yaw control to face command direction
			right2d = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
			target2d = new Vector3(command.x, 0f, command.z);
			
			// update hold values
			_hold = command;
		}
			
		
		Draw.Instance.Bearing(transform.position, right2d, Color.blue);
		Draw.Instance.Bearing(transform.position, target2d, Color.green);
		
			
		// control the local yaw velocity so that ArDrone faces command direction
		float yawAngleError = Vector3.Angle(right2d, target2d);
		float yawCosine = Mathf.Cos(yawAngleError * Mathf.Deg2Rad); // 1:RotateRight, -1:RotateLeft
		float localYawVelocity = transform.InverseTransformDirection(rigidbody.angularVelocity).y;
		float yawForce = (yawCosine*maxYawVelocity - localYawVelocity) * yawKp;
		_controlTorque = transform.rotation * new Vector3(0f, yawForce, 0f);


		// this code starts to work sensibly: it tilts toward the command vector
		// but as tilt increases, the tilt forces increase more, and it flips over
		
		// there may be a simpler method, similar to yaw control above, but for pitch and roll, too
		// https://ghowen.me/build-your-own-quadcopter-autopilot/
		// something like this:
		//hal.rcout->write(MOTOR_FL, rcthr - roll_output - pitch_output - yaw_output);
		//hal.rcout->write(MOTOR_BL, rcthr - roll_output + pitch_output + yaw_output);
		//hal.rcout->write(MOTOR_FR, rcthr + roll_output - pitch_output + yaw_output);
		//hal.rcout->write(MOTOR_BR, rcthr + roll_output + pitch_output - yaw_output);
		
		// calculate propeller thrust vectors
		Vector3 fLeftAxis = (transform.forward - transform.right).normalized;	// local axis along frontleft/backright propellers
		Vector3 fRightAxis = (transform.forward + transform.right).normalized;	// local axis along frontright/backleft propellers
		
		
		 fLeftAngleError = Vector3.Angle(fLeftAxis, target2d);
		float fLeftCosine = Mathf.Cos(fLeftAngleError * Mathf.Deg2Rad);		// 1:MoveForwardLeft, -1:MoveBackRight
		 fLeftTilt = Vector3.Angle(fLeftAxis, Vector3.up) - 90f;		// tilt angle against gravity around frontleft axis
		float fLeftForce = (-1f*fLeftCosine*maxTilt - fLeftTilt) * tiltKp;	// calculate force
		 fLeftForce = Mathf.Max(0f, fLeftForce);						// thrust only one direction
		_flControlThrust = FL.up * fLeftForce;
		
		float bRightForce = (fLeftCosine*maxTilt - fLeftTilt) * tiltKp;
		bRightForce = Mathf.Max(0f, bRightForce);
		_brControlThrust = BR.up * bRightForce;
		
		
		 fRightAngleError = Vector3.Angle(fRightAxis, target2d);
		float fRightCosine = Mathf.Cos(fRightAngleError * Mathf.Deg2Rad);// 1:MoveForwardRight, -1:MoveBackLeft
		 fRightTilt = Vector3.Angle(fRightAxis, Vector3.up) - 90f;	// tilt angle against gravity around frontright axis
		float fRightForce = (-1f*fRightCosine*maxTilt - fRightTilt) * tiltKp;
		fRightForce = Mathf.Max(0f, fRightForce);
		_frControlThrust = FR.up * fRightForce;
		
		float bLeftForce = (fRightCosine*maxTilt - fRightTilt) * tiltKp;
		bLeftForce = Mathf.Max(0f, bLeftForce);
		_blControlThrust = BL.up * bLeftForce;
		
		Draw.Instance.Bearing(FL.position, _flControlThrust, Color.magenta);
		Draw.Instance.Bearing(BR.position, _brControlThrust, Color.magenta);
		Draw.Instance.Bearing(FR.position, _frControlThrust, Color.magenta);
		Draw.Instance.Bearing(BL.position, _blControlThrust, Color.magenta);
		
		Debug.DrawRay(FL.position, _flControlThrust, Color.magenta);
		Debug.DrawRay(BR.position, _brControlThrust, Color.magenta);
		Debug.DrawRay(FR.position, _frControlThrust, Color.magenta);
		Debug.DrawRay(BL.position, _blControlThrust, Color.magenta);
		
	}
	
	// apply control forces
	void FixedUpdate() {
		if(_robot.moveEnabled)  {
			rigidbody.AddForceAtPosition(_flControlThrust, FL.position);
			rigidbody.AddForceAtPosition(_frControlThrust, FR.position);
			rigidbody.AddForceAtPosition(_blControlThrust, BL.position);
			rigidbody.AddForceAtPosition(_brControlThrust, BR.position);
			rigidbody.AddTorque(_controlTorque, ForceMode.Acceleration);
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
