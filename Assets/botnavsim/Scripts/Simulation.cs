using UnityEngine;
using System.Collections;

// This is a manager object used to overlook the running of a simulation.
public class Simulation : MonoBehaviour {

	// Singleton pattern
	public static Simulation Instance;

	// static class members (for easy access in other classes)
	public static GameObject robot;
	public static GameObject destination;
	public static CameraPerspective camPersp;
	public static CameraType camType;
	public static Robot botscript;
	public static bool isRunning = false;

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

	// Time variables used to calculate Simulation.time
	private static float startTime;
	private static float stopTime;

	// Simulation.startDistance
	/// <summary>
	/// Distance from robot start position and destination.
	/// </summary>
	private static float startDistance;
	
	public void StartSimulation() {
		
		robot.transform.position 
			= _astar.graphData.RandomUnobstructedNode().position;
		destination.transform.position 
			= _astar.graphData.RandomUnobstructedNode().position;
		
		botscript.moveEnabled = true;
		botscript.NavigateToDestination();
		isRunning = true;
		startDistance = botscript.distanceToDestination;
		startTime = Time.time;
	}
	
	public void StopSimulation() {
		robot.rigidbody.velocity = Vector3.zero;
		botscript.moveEnabled = false;
		isRunning = false;
	}
	
	private Astar _astar;
	private bool _hideMenu;
	private bool _autoRepeat;

	void Awake() {
		if (Instance) {
			Destroy(this.gameObject);
		}
		else {
			Instance = this;
		}
		_astar = GetComponent<Astar>();

	}
	
	void Start() {
		robot = GameObject.Find("Bot");
		destination = GameObject.Find("Destination");
		camPersp = Camera.main.GetComponent<CameraPerspective>();
		camType = Camera.main.GetComponent<CameraType>();
		
		if (robot) 
			botscript = robot.GetComponent<Robot>();
		else 
			Debug.LogError("Bot not found.");

		botscript.destination = destination.transform;
		camPersp.perspective = CameraPerspective.Perspective.Birdseye;
		camType.type = CameraType.Type.Hybrid;
		
		StopSimulation();
	}
	
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			camPersp.CyclePerspective();
		}
		if (isRunning) {
			if (botscript.atDestination) {
				StopSimulation();
				if (_autoRepeat) StartCoroutine(StartAgain());
			}
			if (botscript.isStuck && _autoRepeat) {
				Debug.LogWarning("Robot thinks its stuck. Restarting...");
				StopSimulation();
				StartCoroutine(StartAgain());
			}

		}
	}

	IEnumerator StartAgain() {
		yield return new WaitForSeconds(3f);
		StopSimulation();
		StartSimulation();
	}

	void OnGUI() {
		// controls
		float top = 0f, left = 0f, width = 250f, height = 100f;
		Rect rect = new Rect(left, top, width, height);
		GUILayout.Window(0, rect, WindowControls, "A* Search Demo");

	}
	
	void WindowControls(int windowID) {
		
		GUILayout.Label("Simulation time: " + time);
		GUILayout.Label("Start distance: " + startDistance);
		GUILayout.Label(botscript.description);
		
		if (_hideMenu) {
			if (GUILayout.Button("Show Menu")) {
				_hideMenu = false;
			}
			return;
		}
		
		if (GUILayout.Button ("Hide Menu")) {
			_hideMenu = true;
		}
		
		if(GUILayout.Button("Start")) {
			if (isRunning) StopSimulation();
			StartSimulation();
		}
		if (GUILayout.Button("Change Camera Mode")) {
			camType.CycleType();
		}
		if (GUILayout.Button("Change camera Perspective")) {
			camPersp.CyclePerspective();
		}
		GUILayout.Label("Viewing from: " + camPersp.perspective.ToString());
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Timescale");
		Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 3f);
		GUILayout.EndHorizontal();
		/* commented out for release to avoid a fatal inf loop bug somewhere...
		bool steps = robot.GetComponent<Astar>().showsteps;
		steps = GUILayout.Toggle(steps,"Show steps");
		robot.GetComponent<Astar>().showsteps = steps;
		*/
		bool manual = botscript.manualControl;
		manual = GUILayout.Toggle(manual,"Manual Control");
		botscript.manualControl = manual;

		_autoRepeat = GUILayout.Toggle(_autoRepeat, "Auto Repeat");

		if(GUILayout.Button("Change Scene")) {
			if (isRunning) StopSimulation();
			int level = Application.loadedLevel;
			if (++level > Application.levelCount-1) 
				level = 0;
			Application.LoadLevel(level); 
		}
	}
}
