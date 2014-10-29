using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Robot : MonoBehaviour {

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool 		manualControl = false;
	public bool 		moveEnabled;
	public float 		maxSpeed = 10f;
	public Sensor[] 	sensors;
	public Transform 	destination;
	
	
	// private members
	// ~-~-~-~-~-~-~-~-
	[System.NonSerialized]
	private INavigation _navigation;
	private Vector3 	_move;			// the move command applied to our rigidbody
	private Vector3 	_manualMove;
	private Vector3?	_destination;	// used to automatically stop when near a target location
	private float 		_stopDistance;	// how close the robot will get to _destination before stopping
	private int 		_snsrIndex;		// circular index for sensor array
	
	// public properties
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Gets depth data from the next sensor in a circular
	/// indexed array of sensors
	/// </summary>
	/// <value>Depth data from the next sensor in a circular
	/// indexed array of sensors</value>
	public Vector3? nextSensorData {
		get {
			Vector3? data = sensors[_snsrIndex].data;
			if (++_snsrIndex >= sensors.Length) _snsrIndex = 0;
			return data;
		}
	}
	
	public string description {
		get {
			int distance = Mathf.RoundToInt(
				Vector3.Distance(transform.position,destination.position));
			
			string desc = "Number of sensors: " + sensors.Length.ToString() + "\n";
			desc += "Robot speed: " + maxSpeed.ToString() + "\n";
			desc = "Distance remaining: " + distance.ToString() + "\n";
			return desc;
		}
	}
	
	public float distanceToDestination {
		get {
			if (!destination) return 0f;
			else return Vector3.Distance(transform.position, destination.position);
		}
	}
	
	// public methods
	// ~-~-~-~-~-~-~-~-
	
	public void NavigateToDestination() {
		_navigation.SetDestination(destination.position);
	}
	
	public void Move(Vector3 direction, float speedpc = 1f) {
		moveEnabled = true;
		_move = direction.normalized * maxSpeed * speedpc;
		_destination = null;
	}
	
	public void MoveTo(Vector3 location, float speedpc = 1f, float stopDistance = 1f) {
		moveEnabled = true;
		Vector3 direction = location - transform.position;
		_move = direction.normalized * maxSpeed * speedpc;
		_destination = location;
		_stopDistance = stopDistance;
	}
	
	public void InitialiseSensors() {
		sensors = GetComponentsInChildren<Sensor>();
	}
	
	// private methods
	// ~-~-~-~-~-~-~-~-
	private void Awake() {
		InitialiseSensors();
		_navigation = GetComponent(typeof(INavigation)) as INavigation;
	}
	
	private void Update() {
		if (manualControl) {
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis ("Vertical");
			_manualMove.x = x * maxSpeed;
			_manualMove.z = y * maxSpeed;
		}
		else if (Simulation.isRunning){
			if (destination.hasChanged) {
				_navigation.SetDestination(destination.position);
				destination.hasChanged = false;
			}
			Vector3? data = nextSensorData;
			if (data.HasValue) {
				_navigation.DepthData(transform.position, 
					transform.position + data.Value, true);
			}
			_move = _navigation.MoveDirection(transform.position);
		}
	}
	
	private void FixedUpdate() {
		bool canMove = moveEnabled;
		Vector3 move = manualControl ? _manualMove : _move;
		if (_destination.HasValue) {
			if (Vector3.Distance(_destination.Value, transform.position) < _stopDistance) {
				canMove = false;
			}
		}
		if (canMove) {
			Vector3 force = move * rigidbody.mass * rigidbody.drag * maxSpeed;
			rigidbody.AddForce(force);
		}
		Debug.DrawRay(transform.position, move, Color.green);
	}
	
}
