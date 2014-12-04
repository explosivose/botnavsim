using UnityEngine;
using System.Collections;

// This is a manager object used to overlook the running of a simulation.
public class Simulation : MonoBehaviour {

	public enum State {
		preSimulation,
		simulating,
		postSimulation
	}
	

	// Singleton pattern
	public static Simulation Instance;

	// static class members (for easy access in other classes)
	public static State state = State.preSimulation;
	public static GameObject robot {
		get { return _robot; }
		set {
			if(_robot) _robot.transform.Recycle();
			_robot = value;
			botscript = _robot.GetComponent<Robot>();
			botscript.destination = destination.transform;
		}
	}
	public static INavigation navigation;
	public static GameObject destination;
	public static Robot botscript;
	public static bool isRunning {
		get { return state == State.simulating; }
	}
	public static Bounds bounds;
	public static bool isReady {
		get { return navigation != null && robot != null; }
	}
	public static bool autoRepeat;
	
	// Simulation.time
	/// <summary>
	/// Time since robot started searching for destination.
	/// </summary>
	/// <value>Time (seconds) since robot started searching for destination.</value>
	public static float time {
		get {
			if (isRunning) stopTime = Time.time;
			return stopTime - startTime;
		}
	}

	private static GameObject _robot;

	// Time variables used to calculate Simulation.time
	private static float startTime;
	private static float stopTime;

	// Simulation.startDistance
	/// <summary>
	/// Distance from robot start position and destination.
	/// </summary>
	private static float startDistance;
	
	public static void Run() {
		
		if (botscript.navigation == null)
			botscript.navigation = navigation;
		
		//robot.transform.position 
		//	= _astar.graphData.RandomUnobstructedNode().position;
		destination.transform.position 
			= Instance.astar.graphData.RandomUnobstructedNode().position;
		
		botscript.moveEnabled = true;
		botscript.NavigateToDestination();
		state = State.simulating;
		startDistance = botscript.distanceToDestination;
		startTime = Time.time;
	}
	
	public static void Stop() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			botscript.moveEnabled = false;
		}
		if (isRunning) {
			state = State.postSimulation;
		}
		else {
			state = State.preSimulation;
		}
	}
	
	public AstarNative astar;
	private bool _hideMenu;

	void Awake() {
		if (Instance) {
			Destroy(this.gameObject);
		}
		else {
			Instance = this;
		}
		astar = GetComponent<AstarNative>();

	}
	
	void Start() {
		bounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach(Renderer r in FindObjectsOfType<Renderer>())
			bounds.Encapsulate(r.bounds);
		
		destination = GameObject.Find("Destination");
		
		Stop();
	}
	
	void Update() {
		if (isRunning) {
			if (botscript.atDestination) {
				Stop();
				if (autoRepeat) StartCoroutine(StartAgain());
			}
			else if (botscript.isStuck && autoRepeat) {
				Debug.LogWarning("Robot thinks its stuck. Restarting...");
				Stop();
				StartCoroutine(StartAgain());
			}

		}
	}

	IEnumerator StartAgain() {
		yield return new WaitForSeconds(3f);
		Stop();
		Run();
	}
		
	void OnDrawGizmos() {
		if (isRunning)
			Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
