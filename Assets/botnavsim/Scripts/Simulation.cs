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
	public static Settings settings = new Settings();

	// Singleton pattern
	public static Simulation Instance;

	// static class members (for easy access in other classes)
	public static State state = State.preSimulation;
	public static int testNumber;
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
	public static bool preSimulation {
		get { return state == State.preSimulation; }
	}
	public static bool isRunning {
		get { return state == State.simulating; }
	}
	public static bool isFinished {
		get { return state == State.finished; }
	}
	public static Bounds bounds;
	public static bool isReady {
		get {
			return settings.robotName != "<none>" && settings.navigationAssemblyName != "<none>";
		}
	}
	public static bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) Time.timeScale = 0f;
			else Time.timeScale = timeScale;
		}
	}
	
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
	
	public static float timeScale {
		get { return _timeScale; }
		set {
			_timeScale = value;
			Time.timeScale = value;
		}
	}

	private static GameObject _robot;

	// Time variables used to calculate Simulation.time
	private static float startTime;
	private static float stopTime;
	private static bool _paused;
	private static float _timeScale;
	
	// Simulation.startDistance
	/// <summary>
	/// Distance from robot start position and destination.
	/// </summary>
	private static float startDistance;
	
	public static void Run() {
		//Application.LoadLevel(settings.levelIndex);
		BotLoader.SetRobot(settings.robotName);
		
		if (botscript.navigation == null)
			botscript.navigation = navigation;

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
			if (botscript.atDestination) {
				if (testNumber >= settings.numberOfTests) {
					Stop();
				}
				else {
					if (settings.repeatOnNavObjectiveComplete) {
						NextTest();
					} 
				}

			}
			else if (botscript.isStuck && settings.repeatOnRobotIsStuck) {
				Debug.LogWarning("Robot thinks its stuck. Restarting...");
				NextTest();
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
