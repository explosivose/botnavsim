using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is a manager object used to overlook the running of a simulation.
public class Simulation : MonoBehaviour {

	public enum State {
		preSimulation,
		simulating,
		stopped,
		finished
	}
	
	[System.Serializable]
	public class Settings {
		public string title = "Simulation";
		public string environmentName = "<none>";
		public string navigationAssemblyName = "<none>";
		public string robotName = "<none>";
		public int numberOfTests = 1;
		public int maximumTestTime = 60;
		public bool randomizeDestination = false;
		public bool randomizeOrigin = false;
		public bool continueOnNavObjectiveComplete = false;
		public bool continueOnRobotIsStuck = false;
		public float initialTimeScale = 1f;
		public bool isValid {
			get {
				bool v = true;
				v &= environmentName != "<none>";
				v &= navigationAssemblyName != "<none>";
				v &= robotName != "<none>";
				v &= numberOfTests > 0;
				return v;
			}
		}
		public string name {
			get {
				return robotName + "|" + navigationAssemblyName + "|" + environmentName;
			}
		}
		public string summary {
			get {
				string s = "";
				s += "Number of tests: " + numberOfTests;
				s += "\nRobot: " + robotName;
				s += "\nNavigation Assembly: " + navigationAssemblyName;
				s += "\nEnvironment: " + environmentName;
				if (randomizeDestination)
					s += "\nRandom destination";
				if (randomizeOrigin)
					s += "\nRandom origin.";
				if (continueOnRobotIsStuck)
					s += "\nAuto repeat if robot gets stuck.";
				
				return s;
			}
		}
		public System.DateTime datetime {get; private set;}
		public string date {
			get {
				return datetime.ToShortDateString();
			}
		}
		public string time {
			get {
				return datetime.ToShortTimeString();
			}
		}
		
		public Settings() {
			datetime = System.DateTime.Now;
		}
	}
	
	// Singleton pattern
	public static Simulation Instance;
	
	
	/** Static  Properties **/
	
	// Simulation state (enumeration)
	public static State state {get; set;}
	
	// Settings for the current simulation (as specified by UI_setup)
	public static Settings settings = new Settings();

	// List of settings to iterate through in batch mode
	public static List<Settings> batch = new List<Settings>();
	
	// Current simulation number (1 to batch.Count)
	public static int simulationNumber {get; set;}
	
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
	
	// Reference to the environment
	public static GameObject environment {
		get; set;
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
	public static bool isStopped {
		get { return state == State.stopped; }
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
	
	/** Static Methods **/
	
	// Start the simulation 
	public static void Begin() {
		simulationNumber = 0;
		NextSimulation();
	}
	
	// Start the next simulation
	public static void NextSimulation() {
		Log.Stop();
		if (++simulationNumber > batch.Count) {
			simulationNumber--;
			End();
			return;
		}
		Instance.StartCoroutine(StartSimulationRoutine());
	}
	
	// Start the next test in the simulation
	public static void NextTest() {
		Log.Stop();
		if (++testNumber > settings.numberOfTests) {
			testNumber--;
			NextSimulation();
			return;
		}
		
		Instance.StartCoroutine(StartTestRoutine());
	}
	
	// Stop the current test
	public static void StopTest() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		state = State.stopped;
	}
	
	// Stop the current simulation
	public static void StopSimulation() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		state = State.stopped;
	}
	
	// Stop the simulation
	public static void End() {
		if (robot) {
			robot.rigidbody.velocity = Vector3.zero;
			robot.moveEnabled = false;
		}
		state = State.finished;
		Log.Stop();
	}
	
	// Routine for starting a new test
	private static IEnumerator StartTestRoutine() {
		StopTest();
		yield return new WaitForSeconds(1f);
		if (settings.randomizeOrigin)
			robot.transform.position = RandomInBounds();
		if (settings.randomizeDestination)
			destination.transform.position = RandomInBounds();
		yield return new WaitForSeconds(1f);
		state = State.simulating;
		_startTime = Time.time;
		Log.Start();
		robot.moveEnabled = true;
		robot.NavigateToDestination();
	}
	
	// Routine for starting a new simulation
	private static IEnumerator StartSimulationRoutine() {
		StopSimulation();
		settings = batch[simulationNumber-1];
		if (environment) environment.transform.Recycle();
		environment = EnvLoader.LoadEnvironment(settings.environmentName);
		SetBounds();
		destination.transform.position = RandomInBounds();
		Camera.main.transform.parent = null;
		robot = BotLoader.LoadRobot(settings.robotName);
		robot.navigation = NavLoader.LoadPlugin(settings.navigationAssemblyName);
		timeScale = settings.initialTimeScale;
		testNumber = 0;
		NextTest();
		yield break;
	}
	
	// Set the simulation bounds to encapsulate all renderers in scene
	private static void SetBounds() {
		bounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach(Renderer r in environment.GetComponentsInChildren<Renderer>())
			bounds.Encapsulate(r.bounds);
	}
	
	// Return a random position inside the simulation bounds
	private static Vector3 RandomInBounds() {
		Vector3 v = bounds.min;
		v.x += Random.Range(0f, bounds.max.x);
		v.y += bounds.max.y;
		v.z += Random.Range(0f, bounds.max.z);
		RaycastHit hit;
		if (Physics.Raycast(v, Vector3.down, out hit, 100f)) {
			v = hit.point + hit.normal;
		}
		return v;
	}
	
	/** Instance Members **/
	
	public AstarNative astar;
	private bool _hideMenu;

	/** Instance Methods **/

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
		destination = GameObject.Find("Destination");
	}
	
	void Update() {
		if (isRunning) {
			// check for conditions to end the test
			if (robot.atDestination && settings.continueOnNavObjectiveComplete) {
				Debug.Log("Simulation: nav objective complete!");
				NextTest();
			}
			if (robot.isStuck && settings.continueOnRobotIsStuck) {
				Debug.LogWarning("Simulation: Robot appears to be stuck! Skipping test.");
				NextTest();
			}
			if (settings.maximumTestTime > 0 && time > settings.maximumTestTime) {
				Debug.LogWarning("Simulation: Max test time exceeded! Skipping test.");
				NextTest();
			}

		}
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
	
	void OnApplicationQuit() {
		Log.Stop();
	}
}
