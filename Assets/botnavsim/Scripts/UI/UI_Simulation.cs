using UnityEngine;
using System.Collections;

public class UI_Simulation : MonoBehaviour {

	public UI_window windowSim = new UI_window();
	public UI_window windowCam = new UI_window();
	private GUISkin _style;
	private Rect _rectSim;
	private Rect _rectCam;
	private bool _hideMenu = true;
	private bool _showCameraMenu;
	private CameraPerspective _camPersp;
	private CameraType _camType;
	
	
	void Start() {
		_style = Resources.Load<GUISkin>("GUI_style");
		_rectSim = new Rect(windowSim.Left, windowSim.Top, windowSim.Width, windowSim.Height);
		_rectCam = new Rect(windowCam.Left, windowCam.Top, windowCam.Width, windowCam.Height);
		_camPersp = Camera.main.GetComponent<CameraPerspective>();
		_camType = Camera.main.GetComponent<CameraType>();
		_camPersp.perspective = CameraPerspective.Perspective.Birdseye;
		_camType.type = CameraType.Type.Hybrid;
	}
	
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			_camPersp.CyclePerspective();
		}
	}
	
	void OnGUI() {
		GUI.skin = _style;
		int i = 100;
		if (Simulation.preSimulation) return;
		
		_rectSim = GUILayout.Window(i++, _rectSim, SimulationWindow, "Simulation Settings");
		if (_showCameraMenu) {
			_rectCam.y = _rectSim.y;
			_rectCam.x = _rectSim.x + _rectSim.width;
			_rectCam = GUILayout.Window(i++, _rectCam, CameraMenu, "Camera");
		}
			
		
	}
	
	void SimulationWindow(int windowID) {
		float leftWidth = 150f;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Time: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.time.ToString("G4"));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Test Number: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.testNumber + " of " + Simulation.settings.numberOfTests);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Robot: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.settings.robotName);
		GUILayout.EndHorizontal();
		
		bool randomDest = Simulation.settings.randomizeDestination;
		bool randomStart = Simulation.settings.randomizeOrigin;
		bool repeatOnComplete = Simulation.settings.repeatOnNavObjectiveComplete;
		bool repeatOnStuck = Simulation.settings.repeatOnRobotIsStuck;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Random Destination: ", GUILayout.Width(leftWidth));
		randomDest = GUILayout.Toggle(randomDest,"");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Random Start: ", GUILayout.Width(leftWidth));
		randomStart = GUILayout.Toggle(randomStart,"");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on complete: " , GUILayout.Width(leftWidth));
		repeatOnComplete = GUILayout.Toggle(repeatOnComplete,"");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on stuck: ", GUILayout.Width(leftWidth));
		repeatOnStuck = GUILayout.Toggle(repeatOnStuck, "");
		GUILayout.EndHorizontal();
		
		Simulation.settings.randomizeDestination = randomDest;
		Simulation.settings.randomizeOrigin = randomStart;
		Simulation.settings.repeatOnNavObjectiveComplete = repeatOnComplete;
		Simulation.settings.repeatOnRobotIsStuck = repeatOnStuck;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Camera: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(_camPersp.perspective.ToString() + "/" + _camType.type.ToString())) {
			_showCameraMenu = true;
		}
		GUILayout.EndHorizontal();
		
		if (Simulation.isRunning) {
			if (Simulation.paused) {
				if (GUILayout.Button("Play"))
					Simulation.paused = false;
			}
			else {
				if (GUILayout.Button("Pause"))
					Simulation.paused = true;
			}
			if (GUILayout.Button("Stop"))
				Simulation.Stop();
			if (GUILayout.Button("Next Test"))
				Simulation.NextTest();
		}
		if (Simulation.isFinished) {
			if  (GUILayout.Button("Start Again")) {
				Simulation.Run();
			}
			if (GUILayout.Button("New Simulation...")) {
				Simulation.state = Simulation.State.preSimulation;
			}
		}
	}
	
	void CameraMenu(int windowID) {
		if (GUILayout.Button(_camType.type.ToString())) {
			_camType.CycleType();
		}
		if (GUILayout.Button(_camPersp.perspective.ToString())) {
			_camPersp.CyclePerspective();
		}
	}
	
	void WindowControls(int windowID) {
		
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
			if (Simulation.isRunning) Simulation.Stop();
			Simulation.Run();
		}
		if (GUILayout.Button("Change Camera Mode")) {
			_camType.CycleType();
		}
		if (GUILayout.Button("Change camera Perspective")) {
			_camPersp.CyclePerspective();
		}
		GUILayout.Label("Viewing from: " + _camPersp.perspective.ToString());
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Timescale");
		Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 3f);
		GUILayout.EndHorizontal();
		/* commented out for release to avoid a fatal inf loop bug somewhere...
		bool steps = robot.GetComponent<Astar>().showsteps;
		steps = GUILayout.Toggle(steps,"Show steps");
		robot.GetComponent<Astar>().showsteps = steps;
		*/
		bool manual = Simulation.botscript.manualControl;
		manual = GUILayout.Toggle(manual,"Manual Control");
		Simulation.botscript.manualControl = manual;
		
		//Simulation.autoRepeat = GUILayout.Toggle(Simulation.autoRepeat, "Auto Repeat");
		
		if (GUILayout.Button("Quit")) {
			Application.Quit();
		}
		
		GUILayout.Label("Simulation time: " + Simulation.time);
		GUILayout.Label(Simulation.botscript.description);
		
	}
}
