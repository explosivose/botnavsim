using UnityEngine;
using System.Collections;

// This is a manager object used to overlook the running of a simulation.
public class Simulation : MonoBehaviour {

	public enum State {
		preSimulation,
		simulating,
		finished
	}
	
	[System.Serializable]
	public class Settings {
		public int levelIndex;
		public string navigationAssemblyName = "<none>";
		public string robotName = "<none>";
		public int numberOfTests = 1;
		public bool randomizeDestination = false;
		public bool randomizeOrigin = false;
		public bool repeatOnNavObjectiveComplete = false;
		public bool repeatOnRobotIsStuck = false;
		public float initialTimeScale = 1f;
		public string summary {
			get {
				string s = "";
				s += "Number of tests: " + numberOfTests;
				s += "\nRobot: " + robotName;
				s += "\nNavigation Assembly: " + navigationAssemblyName;
				if (randomizeDestination)
					s += "\nRandom destination";
				if (randomizeOrigin)
					s += "\nRandom origin.";
				if (repeatOnRobotIsStuck)
					s += "\nAuto repeat if robot gets stuck.";
				
				return s;
			}
		}
	}
	
	// Singleton pattern
	public static Simulation Instance;
	
	// Settings for the current simulation (as specified by UI_setup)
	public static Settings settings = new Settings();

	// static class members (for easy access in other classes)
	
	// Simulation state (enumeration)
	public static State state {get; set;}
	
	// Current test number (1 to settings.numberOfTests)
	public static int testNumber {get; set;}
	
	// Reference to the robot monobehaviour
	public static Robot robot {
		get { return _robot; }
		set {
			if(_robot) _robot.transform.Recycle();
			_robot = value;
			robot.destination = destination.transform;
		}
	}

	// Reference to the destination
	public static GameObject destination { get; set; }
	
	// Simulation states
	public static bool preSimulation {
		get { return state == State.preSimulation; }
	}
	public static bool isRunning {
		get { return state == State.simulating; }
	}
	public static bool isFinished {
		get { return state == State.finished; }
	}
	
	// Simulation bounds (search space for INavigation)
	public static Bounds bounds;
	
	// is the simulation ready to begin?
	public static bool isReady {
		get {
			return settings.robotName != "<none>" && settings.navigationAssemblyName != "<none>";
		}
	}
	
	// is the simulation paused?
	public static bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) Time.timeScale = 0f;
			else Time.timeScale = timeScale;
		}
	}
	

	// Time (seconds) since robot started searching for destination.
	public static float time {
		get {
			if (isRunning) _stopTime = Time.time;
			return _stopTime - _startTime;
		}
	}
	
	// Simulation time scale 
	public static float timeScale {
		get { return _timeScale; }
		set {
			_timeScale = value;
			Time.timeScale = value;
		}
	}

	// Time variables used to calculate Simulation.time
	private static float _startTime;
	private static float _stopTime;
	
	private static Robot _robot;
	private static bool _paused;
	private static float _timeScale;
	
	public static void Begin() {
		//Application.LoadLevel(settings.levelIndex);
		Camera.main.transform.parent = null;
		robot = BotLoader.LoadRobot(settings.robotName);
		
		robot.navigation = NavLoader.LoadPlugin(settings.navigationAssemblyName);

		timeScale = settings.initialTimeScale;
		testNumber = 0;
		NextTest();
	}
	
	public static void NextTest() {
		testNumber++;
		if (settings.randomizeOrigin)
			robot.transform.position 
				= Instance.astar.graphData.RandomUnobstructedNode().position;
		if (settings.randomizeDestination)
			destination.transform.position 
				= Instance.astar.graphData.RandomUnobstructedNode().position;
		
		robot.moveEnabled = true;
		robot.NavigateToDestination();
		state = State.simulating;
		_startTime = Time.time;
	}
	
	public static void Stop() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		if (isRunning) {
			state = State.finished;
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
			if (robot.atDestination) {
				if (testNumber >= settings.numberOfTests) {
					Stop();
				}
				else {
					if (settings.repeatOnNavObjectiveComplete) {
						NextTest();
					} 
				}

			}
			else if (robot.isStuck && settings.repeatOnRobotIsStuck) {
				Debug.LogWarning("Robot thinks its stuck. Restarting...");
				NextTest();
			}

		}
	}

	IEnumerator StartAgain() {
		yield return new WaitForSeconds(3f);
		Stop();
		Begin();
	}
		
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
