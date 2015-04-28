using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// Robot class bridges the gap between sensors and INavigation. The navigation 
/// direction is cached in Robot.navigationCommand. This class does not implement
/// any physics simulation. Robot locomotion models are implemented
/// in other classes that require this component for communications and data. 
/// </summary>
public class Robot : MonoBehaviour, IObservable {

	// sensor callback method type
	public delegate void SensorData(ProximityData data);

	// public fields
	// ~-~-~-~-~-~-~-~-
	public bool 		manualControl;
	public bool 		moveEnabled;
	public float		stopDistance;	// how close the robot will get to destination before stopping
	public Sensor[] 	sensors;
	public Transform 	destination;	
	public Vector3 		centerOfMassOffset;
	
	// private fields
	// ~-~-~-~-~-~-~-~-
	
	private INavigation _navigation;	// reference to the navigation assembly
	private Vector3 	_move;			// the move command from the navigation assembly
	private int 		_snsrIndex;		// circular index for sensor array
	private int 		_stuckCounter;
	private Vector3[]	_positions;
	private BotPath 	_path;
	private Vector3 	_size;
	
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
			_navigation.searchBounds = Simulation.Instance.bounds;
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
	/// The cached navigation bearing from INavigation.
	/// </summary>
	/// <value>The move command.</value>
	public Vector3 navigationCommand {
		get; private set;
	}
	
	/// <summary>
	/// Gets a value indicating whether <see cref="navigation"/> found a path to <see cref="destination"/>.
	/// </summary>
	/// <value><c>true</c> if path found; otherwise, <c>false</c>.</value>
	public bool pathFound {
		get {
			return _navigation.pathFound;
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
	/// Gets the bounds of the robot for IObservable
	/// </summary>
	/// <value>The bounds.</value>
	public Bounds bounds {
		get {
			return new Bounds(transform.position, _size);
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
			if (manualControl) {
				return navigationCommand.magnitude;
			}else{
				if (!destination) return 0f;
				else return Vector3.Distance(transform.position, destination.position);
			}

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
	/// Enable sensors, start moving toward destination using INavigation
	/// </summary>
	public void NavigateToDestination() {
		if (_navigation == null) return;
		StartCoroutine( _navigation.SearchForPath(rigidbody.worldCenterOfMass, destination.position) );
		Debug.Log("Path search due to NavigationToDestination() call.");
		foreach(Sensor s in sensors) {
			s.Enable(ReceiveSensorData);
		}
	}
	
	/// <summary>
	/// Reset the robot, disable sensors
	/// </summary>
	public void Reset() {
		_path = new BotPath();
		_positions = new Vector3[30];
		foreach(Sensor s in sensors) {
			s.Disable();
		}
	}
	
	/// <summary>
	/// Sensor data callback passes proximity data to INavigation
	/// </summary>
	/// <param name="data">Data.</param>
	public void ReceiveSensorData(ProximityData data) {
		if (_navigation == null) return;
		// data is recieved here by any enabled sensor in world space
		// transformed into robot local space if necessary
		// and transmitted to INavigation
		if (_navigation.spaceRelativeTo == Space.Self) {
			_navigation.Proximity(
				Vector3.zero,
				transform.InverseTransformDirection(data.direction),
				data.obstructed);
		} else {
			_navigation.Proximity(
				transform.position,
				transform.position + data.direction,
				data.obstructed);
		}
	}
	
	// private methods
	// ~-~-~-~-~-~-~-~-
	
	// called once at the start
	private void Awake() {
		sensors = GetComponentsInChildren<Sensor>();
		StartCoroutine(StuckDetector());
		cameraMount = transform.Find("CameraMount");
		if (!cameraMount) {
			cameraMount = transform;
			Debug.LogWarning("CameraMount object not found on Robot.");
		} 
		// calculate size bounding box for renderers
		Bounds b = new Bounds(Vector3.zero, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
			b.Encapsulate(r.bounds);
		_size = b.size;
		// add center of mass offset
		rigidbody.centerOfMass += centerOfMassOffset;
		// initialise bath plotter
		_path = new BotPath();
		StartCoroutine(RecordPath());
	}
	
	// called every rendered frame
	private void Update() {
	
	
	
		// manual control for testing robots
		if (manualControl) {
			float x = Input.GetAxis("X");
			float y = Input.GetAxis("Y");
			float z = Input.GetAxis("Z");
			
			navigationCommand = new Vector3(x, y, z);

		}
		else if (Simulation.isRunning){
			
			// draw path
			_path.DrawPath();
			
			if (_navigation == null) return;
			// Update INavigation.destination if it has changed.
			if (destination.hasChanged) {
				if (Vector3.Distance(_navigation.destination,destination.position)>0.5f) {
					if (_navigation.spaceRelativeTo == Space.Self) {
						Vector3 dest = transform.InverseTransformPoint(destination.position);
						StartCoroutine( _navigation.SearchForPath(Vector3.zero, dest) );
					}
					else {
						StartCoroutine( _navigation.SearchForPath(rigidbody.worldCenterOfMass, destination.position) );
					}
					Debug.Log("Path search due to destination change.");
				}

				destination.hasChanged = false;
			}
			
			// Read move direction if a path has been found.
			if (_navigation.pathFound) {
				if (navigation.spaceRelativeTo == Space.Self) {
					navigationCommand = _navigation.PathDirection(Vector3.zero);
					navigationCommand = transform.TransformDirection(navigationCommand);
				}
				else {
					navigationCommand = _navigation.PathDirection(transform.position);
				}
				Debug.DrawRay(transform.position, navigationCommand * stopDistance, Color.green);
			}
			else {
				navigationCommand = Vector3.zero;
			}		
		}
	}
	

	
	// draws shapes on screen inside the Unity Editor
	private void OnDrawGizmos() {
		if (_navigation != null) {
			_navigation.DrawGizmos();
		}
			
		// draw center of mass
		Gizmos.color = Color.Lerp(Color.yellow, Color.clear, 0.25f);
		if (Application.isPlaying) {
			Gizmos.DrawSphere(rigidbody.worldCenterOfMass, 0.05f);
		} else {
			Gizmos.DrawSphere(rigidbody.worldCenterOfMass + centerOfMassOffset, 0.05f);
		}
		
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
	
	private IEnumerator RecordPath() {
		Vector3 prev = rigidbody.worldCenterOfMass;;
		_path.AddNode(prev, Simulation.time);
		while (true) {
			// only record a line when bot has moved far enough
			if (Vector3.Distance(prev, rigidbody.worldCenterOfMass) > 0.1f) {
				prev = rigidbody.worldCenterOfMass;
				_path.AddNode(rigidbody.worldCenterOfMass, Simulation.time);
			}
			yield return new WaitForSeconds(Log.timeStep);
		}
	}
}
