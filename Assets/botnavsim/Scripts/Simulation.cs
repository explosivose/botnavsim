using UnityEngine;
using System.Collections;

public class Simulation : MonoBehaviour {

	public static Simulation Instance;
	
	public static GameObject robot;
	public static GameObject destination;
	public static CameraPerspective cam;
	
	public static Robot botscript;
	
	public static bool isRunning = false;
	
	public static float time {
		get {
			if (isRunning) stopTime = Time.time;
			return stopTime - startTime;
		}
	}
	
	private static float startTime;
	private static float stopTime;
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
		cam = Camera.main.GetComponent<CameraPerspective>();
		
		if (robot) 
			botscript = robot.GetComponent<Robot>();
		else 
			Debug.LogError("Bot not found.");
			
		botscript.destination = destination.transform;
		cam.perspective = CameraPerspective.Perspective.Birdseye;
		
		StopSimulation();
	}
	
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			cam.CyclePerspective();
		}
		if (isRunning) {
			if (botscript.distanceToDestination < 1f) {
				StopSimulation();
			}
		}
	}
	
	void OnGUI() {
		// controls
		float top = 0f, left = 0f, width = 250f, height = 200f;
		Rect rect = new Rect(left, top, width, height);
		GUILayout.Window(0, rect, WindowControls, "A* Search Demo");

	}
	
	void WindowControls(int windowID) {
		
		GUILayout.Label("Simulation time: " + time);
		GUILayout.Label("Start distance: " + startDistance);
		GUILayout.Label(botscript.description);
		
		if(GUILayout.Button("Start")) {
			if (isRunning) StopSimulation();
			StartSimulation();
		}
		if (GUILayout.Button("Change camera")) {
			cam.CyclePerspective();
		}
		GUILayout.Label("Viewing from: " + cam.perspective.ToString());
		GUILayout.BeginHorizontal();
		GUILayout.Label("Timescale");
		Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 3f);
		GUILayout.EndHorizontal();
		bool steps = robot.GetComponent<Astar>().showsteps;
		steps = GUILayout.Toggle(steps,"Show steps");
		robot.GetComponent<Astar>().showsteps = steps;
	}
}
