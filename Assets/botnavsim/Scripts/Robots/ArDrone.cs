using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Robot))]
public class ArDrone : MonoBehaviour {
	
	public float maxLateralSpeed;
	public Pid lateralSpeedController;
	public Pid tiltController;
	public float verticalSpeed;
	public float maxThrottle = 0.2f;
	public Pid throttleController;
	public float maxTilt = 25f;
	public float maxYawVelocity = 0.5f;
	public float yawKp;
	public Pid rollController;
	public Pid pitchController;
	
	
	public Transform FL;
	public Transform FR;
	public Transform BL;
	public Transform BR;
	
	private float _actualPitch;
	private float _actualRoll;
	
	private float _targetSpeed;
	private float _targetTilt;
	private float _targetHeight;
	
	private Robot _robot;
	
	private Vector3 _holdRotation;			// maintain this forward direction in yaw control when there is no navigation command
	private Vector3 _holdPosition;
	

	private Vector3 _controlTorque;			// yaw on transform.up axis
	private Vector3 _flControlThrust;		// front left propeller force (transform.down axis)
	private Vector3 _frControlThrust;		// front right propeller force
	private Vector3 _blControlThrust;		// back left propeller force
	private Vector3 _brControlThrust;		// back right propeller force
	
	void Awake() {
		_robot = GetComponent<Robot>();
	}
	
	void Start() {
		_holdPosition = transform.position;
		_targetHeight = transform.position.y;
	}
	
	void Reset() {
		lateralSpeedController.Reset();
		pitchController.Reset();
		rollController.Reset();
		throttleController.Reset();
		tiltController.Reset();
	}
	
	// calculate control forces
	void Update() {
		
		if (!_robot.moveEnabled) {
			Reset();
			_holdPosition = transform.position;
			return;
		}
		
		Vector3 command = _robot.navigationCommand;
		
		Draw.Instance.Bearing(transform.position, command, Color.green);
		Debug.DrawRay(transform.position, command, Color.green);
		
		_targetHeight += command.y * Time.deltaTime * verticalSpeed;
		
		
		
		Vector3 forward2d; 	// transform.forward with no y component
		Vector3 right2d;	// transform.right with no y component
		Vector3 target2d;	// target direction with no y component
		Vector3 yawTarget;	// set to vector3.forward when there is no command
		float displacement2d;
		
		forward2d = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
		right2d = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
		
		// if no command
		if (command.magnitude < 0.1f) {		// no command, replace command vector with hold values
			
			
			target2d = _holdPosition - transform.position;
			target2d.y = 0f;
			yawTarget = Vector3.forward;
			displacement2d = target2d.magnitude;
			
		} else {	// deal with command vector
			
			// X/Z plane target direction
			target2d = new Vector3(command.x, 0f, command.z);
			yawTarget = target2d;
			// update hold previous command 
			_holdRotation = command;
			// keep hold position
			_holdPosition = transform.position;
			displacement2d = command.magnitude;
		}
		
		_targetSpeed = lateralSpeedController.output(0f, - displacement2d);
		_targetSpeed = Mathf.Clamp(_targetSpeed, 0f, maxLateralSpeed);
		_targetTilt = tiltController.output(_targetSpeed, rigidbody.velocity.magnitude);
		_targetTilt = Mathf.Clamp(_targetTilt, -maxTilt, maxTilt);
		
		
		Draw.Instance.Bearing(transform.position, target2d, Color.red);
		Debug.DrawRay(transform.position, target2d, Color.red);
			
		// control the local yaw velocity so that ArDrone faces command direction
		float yawAxisAngle = Vector3.Angle(right2d, yawTarget);
		float yawCosine = Mathf.Cos(yawAxisAngle * Mathf.Deg2Rad); // 1:RotateRight, -1:RotateLeft
		float localYawVelocity = transform.InverseTransformDirection(rigidbody.angularVelocity).y;
		float yawForce = (yawCosine*maxYawVelocity - localYawVelocity) * yawKp;
		_controlTorque = transform.rotation * new Vector3(0f, yawForce, 0f);


		// adapted from here
		// https://ghowen.me/build-your-own-quadcopter-autopilot/

	
		// PITCH
		float forwardAxisAngle = Vector3.Angle(forward2d, target2d);
		float forwardCosine = Mathf.Cos(forwardAxisAngle * Mathf.Deg2Rad);
		_actualPitch = Vector3.Angle(transform.forward, Vector3.up) - 90f; // avoids the 0-360 boundary
		float pitchOutput = pitchController.output(forwardCosine*_targetTilt, _actualPitch);
		// ROLL
		float rightAxisAngle = Vector3.Angle(right2d, target2d);
		float rightCosine = Mathf.Cos(rightAxisAngle * Mathf.Deg2Rad);
		_actualRoll = Vector3.Angle(transform.right, Vector3.up) - 90f;	// avoids the 0-360 boundary
		float rollOutput = rollController.output(rightCosine*_targetTilt, _actualRoll);
		
		

		Draw.Instance.Bearing(transform.position, throttleController.error * Vector3.up, Color.red);
		Debug.DrawRay(transform.position, throttleController.error * Vector3.up, Color.red);
		
		// THROTTLE
		float throttle = throttleController.output(_targetHeight, transform.position.y);
		
		float flForce = Mathf.Clamp(throttle + rollOutput - pitchOutput, 0f, maxThrottle);
		float blForce = Mathf.Clamp(throttle + rollOutput + pitchOutput, 0f, maxThrottle);
		float frForce = Mathf.Clamp(throttle - rollOutput - pitchOutput, 0f, maxThrottle);
		float brForce = Mathf.Clamp(throttle - rollOutput + pitchOutput, 0f, maxThrottle);
		
		
		_flControlThrust = FL.up * flForce;
		_blControlThrust = BL.up * blForce;
		_frControlThrust = FR.up * frForce;
		_brControlThrust = BR.up * brForce;
		

		
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
