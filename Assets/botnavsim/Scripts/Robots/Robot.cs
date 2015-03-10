using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Robot class bridges the gap between sensors and INavigation. The navigation 
/// direction is cached in Robot.moveCommand. This class does not implement
/// any physics simulation. Robot locomotion models are implemented
/// in other classes that require this component for communications and data. 
/// </summary>
public class Robot : MonoBehaviour {

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool 		manualControl;
	public bool 		moveEnabled;
	public float		stopDistance;	// how close the robot will get to _destination before stopping
	public ParamSensor[] 	sensors;
	public Transform 	destination;	
	public Vector3 		centerOfMass;
	
	// private members
	// ~-~-~-~-~-~-~-~-
	
	private INavigation _navigation;
	private Vector3 	_move;			// the move command applied to our rigidbody
	private Vector3?	_destination;	// used to automatically stop when near a target location
	private int 		_snsrIndex;		// circular index for sensor array
	private int 		_stuckCounter;
	private Vector3[]	_positions;

	// public properties
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Interface to the navigation assembly. 
	/// </summary>
	/// <value>When set, the INavigation interface is initialised:
	/// INavigation.searchBounds is set, 
	/// INavigation.origin is set,
	/// INavigation.destination is set.</value>
	public INavigation 	navigation {
		get {
			return _navigation;
		}
		set {
			_navigation = value;
			_navigation.searchBounds = Simulation.bounds;
			if (_navigation.spaceRelativeTo == Space.Self) {
				_navigation.origin = Vector3.zero;
				_navigation.destination = transform.InverseTransformPoint(destination.position);
			}
			else {
				_navigation.origin = transform.position;
				_navigation.destination = destination.position;
			}

		}
	}
	
	/// <summary>
	/// The normalized move command direction from INavigation. 
	/// </summary>
	/// <value>The move command.</value>
	public Vector3 moveCommand {
		get {
			return _move.normalized;
		}
	}
	
	/// <summary>
	/// Gets or sets the transform position.
	/// </summary>
	/// <value>The transform position.</value>
	public Vector3 position {
		get{ return transform.position; }
		set{ transform.position = value; }
	}
	
	/// <summary>
	/// The first-person perspective camera position. 
	/// </summary>
	/// <value>The camera mount position.</value>
	public Transform cameraMount { get; private set; }
	
	/// <summary>
	/// Gets depth data from the next sensor in a circular
	/// indexed array of sensors
	/// </summary>
	/// <value>Depth data from the next sensor in a circular
	/// indexed array of sensors</value>
	public ProximityData nextSensorData {
		get {
			ProximityData data = sensors[_snsrIndex].GetData();
			if (++_snsrIndex >= sensors.Length) _snsrIndex = 0;
			return data;
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

	public bool isStuck {
		get {
			return _stuckCounter > 30;
		}
	}

	public int stuckpc {
		get {
			return Mathf.RoundToInt(100f*(float)_stuckCounter/(float)30);
		}
	}

	// public methods
	// ~-~-~-~-~-~-~-~-
	
	/// <summary>
	/// Start moving toward destination.
	/// </summary>
	public void NavigateToDestination() {
		if (_navigation == null) return;
		StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
	}

	
	// private methods
	// ~-~-~-~-~-~-~-~-
	
	// Populate sensor array
	private void InitialiseSensors() {
		sensors = GetComponentsInChildren<ParamSensor>();
	}
	
	private void Awake() {
		InitialiseSensors();
		StartCoroutine(StuckDetector());
		cameraMount = transform.Find("CameraMount");
		if (!cameraMount) {
			cameraMount = transform;
			Debug.LogWarning("CameraMount object not found on Robot.");
		} 
	}
	
	private void Update() {
		ProximityData data = nextSensorData;
		if (_navigation == null) return;
		
		// update center of mass
		rigidbody.centerOfMass = centerOfMass;
		
		if (manualControl) {
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis ("Vertical");
			_move.x = x;
			_move.z = y;
		}
		else if (Simulation.isRunning){
			
			// Pass sensor data to INavigation
			if (_navigation.spaceRelativeTo == Space.Self) {
				data.direction = transform.InverseTransformDirection(data.direction);
				_navigation.Proximity(Vector3.zero, 
										data.direction, 
										20f,
										data.obstructed);
			}
			else {
				_navigation.Proximity(transform.position, 
				                      transform.position + data.direction, 
				                      20f,
				                      data.obstructed);
			}
			
			// Update INavigation.destination if it has changed.
			if (destination.hasChanged) {
				if (_navigation.spaceRelativeTo == Space.Self) {
					Vector3 dest = transform.InverseTransformPoint(destination.position);
					StartCoroutine( _navigation.SearchForPath(Vector3.zero, dest) );
				}
				else {
					StartCoroutine( _navigation.SearchForPath(transform.position, destination.position) );
				}
				
				destination.hasChanged = false;
			}
			
			// Read move direction if a path has been found.
			if (_navigation.pathFound) {
				if (navigation.spaceRelativeTo == Space.Self) {
					_move = _navigation.PathDirection(Vector3.zero);
					_move = transform.TransformDirection(_move);
				}
				else {
					_move = _navigation.PathDirection(transform.position + Vector3.up);
				}
				Debug.DrawRay(transform.position, _move * stopDistance, Color.green);
			}
			else {
				_move = Vector3.zero;
			}		
		}
	}
	
	private void OnDrawGizmos() {
		if (_navigation != null)
			_navigation.DrawGizmos();
		// draw center of mass
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(rigidbody.worldCenterOfMass, 0.1f);
	}
	
	// detects if robot appears to be stuck taking an average position over time
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
