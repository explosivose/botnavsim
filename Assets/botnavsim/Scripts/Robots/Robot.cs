using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Robot : MonoBehaviour {

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool 		manualControl = false;
	public bool 		moveEnabled;
	public bool 		faceMoveDirection = false;
	public float 		maxSpeed = 10f;
	public float		stopDistance;	// how close the robot will get to _destination before stopping
	public Sensor[] 	sensors;
	public Transform 	destination;	
	

	
	// private members
	// ~-~-~-~-~-~-~-~-
	
	private INavigation _navigation;
	private Vector3 	_move;			// the move command applied to our rigidbody
	private Vector3 	_manualMove;
	private Vector3?	_destination;	// used to automatically stop when near a target location
	private int 		_snsrIndex;		// circular index for sensor array
	private int 		_stuckCounter;
	private Vector3[]	_positions;

	// public properties
	// ~-~-~-~-~-~-~-~-
	
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
	
	public Vector3 moveCommand {
		get {
			return _move.normalized;
		}
	}
	
	/// <summary>
	/// Gets depth data from the next sensor in a circular
	/// indexed array of sensors
	/// </summary>
	/// <value>Depth data from the next sensor in a circular
	/// indexed array of sensors</value>
	public Sensor.ProximityData nextSensorData {
		get {
			Sensor.ProximityData data = sensors[_snsrIndex].data;
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
			return _stuckCounter > 30;
		}
	}

	public int stuckpc {
		get {
			return Mathf.RoundToInt((float)_stuckCounter/(float)30);
		}
	}

	// public methods
	// ~-~-~-~-~-~-~-~-
	
	public void NavigateToDestination() {
		if (_navigation == null) return;
		StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
	}

	
	public void InitialiseSensors() {
		sensors = GetComponentsInChildren<Sensor>();
	}
	
	// private methods
	// ~-~-~-~-~-~-~-~-
	private void Awake() {
		InitialiseSensors();
		_navigation = GetComponent(typeof(INavigation)) as INavigation;
		StartCoroutine(StuckDetector());
	}
	
	private void Update() {
		if (_navigation == null) return;

		
		if (manualControl) {
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis ("Vertical");
			_manualMove.x = x * maxSpeed;
			_manualMove.z = y * maxSpeed;
		}
		else if (Simulation.isRunning){
			Sensor.ProximityData data = nextSensorData;
			_navigation.Proximity(transform.position, 
				transform.position + data.direction, data.obstructed);
			if (destination.hasChanged) {
				StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
				destination.hasChanged = false;
			}
			if (_navigation.pathFound)
				_move = _navigation.PathDirection(transform.position);
		}

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
			Quaternion rotation = Quaternion.LookRotation(move);
			transform.rotation = rotation;//Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 4f);
			Vector3 force = move.normalized * rigidbody.mass * rigidbody.drag * maxSpeed;
			rigidbody.AddForce(force);
		}
		Debug.DrawRay(transform.position, move, Color.green);
	}
	
	void OnDrawGizmos() {
		if (_navigation != null)
			_navigation.DrawGizmos();
	}
	
	private IEnumerator StuckDetector() {
		_positions = new Vector3[30];
		int i = 0;
		while(true) {
			_positions[i] = transform.position;
			if (++i >= _positions.Length) i = 0;
			Vector3 avg = Vector3.zero;
			foreach(Vector3 v in _positions) {
				avg += v;
			}
			avg /= (float)_positions.Length;
			if (Vector3.Distance(avg, transform.position) < 1f && Simulation.isRunning) {
				_stuckCounter++;
			}
			else {
				_stuckCounter = 0;
			}
			yield return new WaitForSeconds(0.3f);
		}
	}
}
