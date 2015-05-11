using UnityEngine;
using System.Collections;

/// <summary>
/// AR Drone control system implementation.
/// Developed with data from http://parrotcontact.emencia.net/website/user-guides/download-user-guides.php?pdf=ar-drone-2/AR-Drone-2_User-guide_Android_UK.pdf
/// See final report for diagram of implementation.
/// </summary>
[RequireComponent(typeof(Robot))]
public class ArDrone : MonoBehaviour {
	
	// public members
	// -~-~-~-~-~-~-~
	
	/// <summary>
	/// The maximum lateral speed allowed by the control system.
	/// </summary>
	public float maxLateralSpeed;
	
	/// <summary>
	/// The lateral speed controller.
	/// </summary>
	public Pid lateralSpeedController;
	
	/// <summary>
	/// The tilt controller.
	/// </summary>
	public Pid tiltController;
	
	/// <summary>
	/// The maximum speed that the vertical target location can change. 
	/// </summary>
	public float verticalSpeed;
	
	/// <summary>
	/// The max throttle of each propeller force. 
	/// </summary>
	public float maxThrottle;
	
	/// <summary>
	/// The throttle controller.
	/// </summary>
	public Pid throttleController;
	
	/// <summary>
	/// The maximum roll and pitch permitted by the control system. (measured in degrees)
	/// </summary>
	[Range(5.0f,30.0f)]
	public float maxTilt;
	
	/// <summary>
	/// If <c>true</c> then AR Drone will rotate to face the command direction vector.
	/// </summary>
	public bool rotateYaw;
	
	/// <summary>
	/// The max yaw rotational velocity.  (measured in degrees per second)
	/// </summary>
	[Range(40.0f,350.0f)]
	public float maxYawVelocity;
	
	/// <summary>
	/// Gain parameter in controlling the yaw torque.
	/// </summary>
	public float yawKp;
	
	/// <summary>
	/// The roll controller.
	/// </summary>
	public Pid rollController;
	/// <summary>
	/// The pitch controller.
	/// </summary>
	public Pid pitchController;
	
	/// <summary>
	/// Reference to the front-left propeller position.
	/// </summary>
	public Transform FL;
	
	/// <summary>
	/// Reference to the front-right propeller position.
	/// </summary>
	public Transform FR;
	
	/// <summary>
	/// Reference to the back-left propeller position.
	/// </summary>
	public Transform BL;
	
	/// <summary>
	/// Reference to the back-right propeller position.
	/// </summary>
	public Transform BR;
	
	private float _actualPitch;				// measured pitch
	private float _actualRoll;				// measured roll
	
	private float _targetSpeed;				// target lateral speed
	private float _targetTilt;				// modified max tilt
	private float _targetRoll;				// target roll
	private float _targetPitch;				// target pitch
	private float _targetHeight;			// target height
	
	private Robot _robot;					// reference to the Robot component 
	
	private Vector3 _command;
	private Vector3 _target2d;				// target direction with no y component
	private Vector3 _holdRotation;			// maintain this forward direction in yaw control when there is no navigation command
	private Vector3 _holdPosition;
	

	private Vector3 _controlTorque;			// yaw on transform.up axis
	private Vector3 _flControlThrust;		// front left propeller force (transform.down axis)
	private Vector3 _frControlThrust;		// front right propeller force
	private Vector3 _blControlThrust;		// back left propeller force
	private Vector3 _brControlThrust;		// back right propeller force
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_robot = GetComponent<Robot>();
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		_holdPosition = transform.position;
		_targetHeight = transform.position.y;
	}
	
	/// <summary>
	/// Reset the PID controllers.
	/// </summary>
	void Reset() {
		lateralSpeedController.Reset();
		pitchController.Reset();
		rollController.Reset();
		throttleController.Reset();
		tiltController.Reset();
		_targetHeight = transform.position.y;
	}
	
	// calculate control forces
	void Update() {
		
		if (!_robot.moveEnabled) {
			Reset();
			_holdPosition = transform.position;
			return;
		}
		
		_command = _robot.navigationCommand;
		
		Draw.Instance.Bearing(transform.position, _command, Color.green);
		Debug.DrawRay(transform.position, _command, Color.green);
		
		_targetHeight += _command.y * Time.deltaTime * verticalSpeed;
		
		Vector3 forward2d = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
		Vector3 right2d = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
		Vector3 yawTarget;	// set to vector3.forward when there is no command
		

		
		// if no command
		if (_command.magnitude < 0.1f) {		// no command, replace command vector with hold values
			
			
			_target2d = _holdPosition - transform.position;
			_target2d.y = 0f;
			yawTarget = Vector3.forward;
			
		} else {	// deal with command vector
			
			// X/Z plane target direction
			_target2d = new Vector3(_command.x, 0f, _command.z);
			yawTarget = _target2d;
			// update hold previous command 
			_holdRotation = _command;
			// keep hold position
			_holdPosition = transform.position;
		}
		

		
		
		Draw.Instance.Bearing(transform.position, _target2d, Color.red);
		Debug.DrawRay(transform.position, _target2d, Color.red);
			
		if (rotateYaw) {
			// control the local yaw velocity so that ArDrone faces command direction
			float yawAxisAngle = Vector3.Angle(right2d, yawTarget);
			float yawCosine = Mathf.Cos(yawAxisAngle * Mathf.Deg2Rad); // 1:RotateRight, -1:RotateLeft
			float localYawVelocity = transform.InverseTransformDirection(rigidbody.angularVelocity).y;
			float yawForce = (yawCosine*maxYawVelocity - localYawVelocity) * yawKp;
			_controlTorque = transform.rotation * new Vector3(0f, yawForce, 0f);
		}


	

		

		Draw.Instance.Bearing(transform.position, throttleController.error * Vector3.up, Color.red);
		Debug.DrawRay(transform.position, throttleController.error * Vector3.up, Color.red);
		
		
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
		if(!_robot.moveEnabled)  return;
		
		float displacement2d = _target2d.magnitude;

		_targetSpeed = lateralSpeedController.output(0f, - displacement2d);
		_targetSpeed = Mathf.Clamp(_targetSpeed, 0f, maxLateralSpeed);
		_targetTilt = tiltController.output(_targetSpeed, rigidbody.velocity.magnitude);
		_targetTilt = Mathf.Clamp(_targetTilt, 0f, maxTilt);
		
		Vector3 forward2d = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
		Vector3 right2d = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
		
		// PITCH
		float forwardAxisAngle = Vector3.Angle(forward2d, _target2d);
		float forwardCosine = Mathf.Cos(forwardAxisAngle * Mathf.Deg2Rad);
		_actualPitch = Vector3.Angle(transform.forward, Vector3.up) - 90f; // avoids the 0-360 boundary
		_targetPitch = forwardCosine*_targetTilt;
		
		// ROLL
		float rightAxisAngle = Vector3.Angle(right2d, _target2d);
		float rightCosine = Mathf.Cos(rightAxisAngle * Mathf.Deg2Rad);
		_actualRoll = Vector3.Angle(transform.right, Vector3.up) - 90f;	// avoids the 0-360 boundary
		_targetRoll = rightCosine*_targetTilt;
		
		
		float pitchOutput = pitchController.output(_targetPitch, _actualPitch);
		float rollOutput = rollController.output(_targetRoll, _actualRoll);
		
		// THROTTLE
		float throttle = throttleController.output(_targetHeight, transform.position.y);
		
		// adapted from here
		// https://ghowen.me/build-your-own-quadcopter-autopilot/
		float flForce = Mathf.Clamp(throttle + rollOutput - pitchOutput, 0f, maxThrottle);
		float blForce = Mathf.Clamp(throttle + rollOutput + pitchOutput, 0f, maxThrottle);
		float frForce = Mathf.Clamp(throttle - rollOutput - pitchOutput, 0f, maxThrottle);
		float brForce = Mathf.Clamp(throttle - rollOutput + pitchOutput, 0f, maxThrottle);
		
		
		_flControlThrust = FL.up * flForce;
		_blControlThrust = BL.up * blForce;
		_frControlThrust = FR.up * frForce;
		_brControlThrust = BR.up * brForce;
		
		rigidbody.AddForceAtPosition(_flControlThrust, FL.position);
		rigidbody.AddForceAtPosition(_frControlThrust, FR.position);
		rigidbody.AddForceAtPosition(_blControlThrust, BL.position);
		rigidbody.AddForceAtPosition(_brControlThrust, BR.position);
		rigidbody.AddTorque(_controlTorque, ForceMode.Acceleration);
		
	}
	

}
