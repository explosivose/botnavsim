using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Robot : MonoBehaviour {

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool 		manualControl = false;
	public bool 		moveEnabled;
	public float 		maxSpeed = 10f;
	public float		stopDistance;	// how close the robot will get to _destination before stopping
	public Sensor[] 	sensors;
	public Transform 	destination;	
	
	public INavigation 	navigation {
		get {
			return _navigation;
		}
		set {
			_navigation = value;
			_navigation.searchBounds = Simulation.bounds;
			_navigation.origin = transform.position;
			_navigation.destination = destination.position;
		}
	}
	
	// private members
	// ~-~-~-~-~-~-~-~-
	//[System.NonSerialized]
	private INavigation _navigation;
	private Vector3 	_move;			// the move command applied to our rigidbody
	private Vector3 	_manualMove;
	private Vector3?	_destination;	// used to automatically stop when near a target location
	private int 		_snsrIndex;		// circular index for sensor array
	private int 		_stuckCounter;

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
			string desc = "";

			if (atDestination) desc += "Arrived at destination!" + "\n";

			desc = "Number of sensors: " + sensors.Length.ToString() + "\n";
			if (isStuck) {
				desc += "\"I think I'm stuck!!\"\n";
			}
			if (_navigation == null) {
				desc += "Navigation Algorithm not loaded...\n";
			}
			else {
				if (!_navigation.pathFound) {
					desc += "Waiting for path...\n";
				}
				else if (!atDestination)  {
					desc += "Robot speed: " + (100f * speed01).ToString("0") + "%\n";
					desc += "Distance remaining: " + distance.ToString() + "\n";
				}
			}
			return desc;
		}
	}

	public bool atDestination {
		get {
			if (!destination) return false;
			float distance = Vector3.Distance(transform.position, destination.position);
			if (distance < stopDistance) return true;
			return false;
		}
	}

	public float distanceToDestination {
		get {
			if (!destination) return 0f;
			else return Vector3.Distance(transform.position, destination.position);
		}
	}

	public float speed01 {
		get {
			return rigidbody.velocity.magnitude/maxSpeed;
		}
	}

	public bool isStuck {
		get {
			return _stuckCounter > 300;
		}
	}

	// public methods
	// ~-~-~-~-~-~-~-~-
	
	public void NavigateToDestination() {
		if (_navigation == null) return;
		StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
	}
	
	public void Move(Vector3 direction, float speedpc = 1f) {
		moveEnabled = true;
		_move = direction.normalized * maxSpeed * speedpc;
		_destination = null;
	}
	
	public void MoveTo(Vector3 location, float speedpc = 1f) {
		moveEnabled = true;
		Vector3 direction = location - transform.position;
		_move = direction.normalized * maxSpeed * speedpc;
		_destination = location;
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
		if (_navigation == null) return;
		Vector3? data = nextSensorData;
		if (data.HasValue) {
			_navigation.Proximity(transform.position, 
			                      transform.position + data.Value, true);
		}
		if (manualControl) {
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis ("Vertical");
			_manualMove.x = x * maxSpeed;
			_manualMove.z = y * maxSpeed;
		}
		else if (Simulation.isRunning){
			if (destination.hasChanged) {
				StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
				destination.hasChanged = false;
			}
			if (_navigation.pathFound)
				_move = _navigation.PathDirection(transform.position);
		}

		if (rigidbody.velocity.magnitude < 1f && Simulation.time > 1f)
			_stuckCounter++;
		else
			_stuckCounter = 0;
	}
	
	private void FixedUpdate() {
		bool canMove = moveEnabled || manualControl;
		Vector3 move = manualControl ? _manualMove : _move;
		if (_destination.HasValue) {
			if (atDestination) {
				canMove = false;
			}
		}
		if (canMove) {
			Vector3 force = move.normalized * rigidbody.mass * rigidbody.drag * maxSpeed;
			rigidbody.AddForce(force);
		}
		Debug.DrawRay(transform.position, move, Color.green);
	}
	
	void OnDrawGizmos() {
		if (_navigation != null)
			_navigation.DrawGizmos();
	}
}
