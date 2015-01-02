using UnityEngine;
using System.Collections;

public class UI_Simulation : MonoBehaviour {

	public UI_window windowSim = new UI_window();
	public UI_window windowCam = new UI_window();
	private GUISkin _style;
	private Rect _rectSim;
	private Rect _rectCam;
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
		
		_rectSim = GUILayout.Window(i++, _rectSim, SimulationWindow, Simulation.settings.title);
		if (_showCameraMenu) {
			_rectCam.y = _rectSim.y;
			_rectCam.x = _rectSim.x + _rectSim.width;
			_rectCam = GUILayout.Window(i++, _rectCam, CameraMenu, "Camera");
		}
			
		
	}
	
	void SimulationWindow(int windowID) {
		float leftWidth = 150f;
		
		if (GUILayout.Button("Exit Application")) {
			Application.Quit();
		}
		
		GUILayout.Space (10);
		
		GUILayout.Label(Simulation.settings.robotName + "\n" +
		                Simulation.settings.environmentName + "\n" + 
		                Simulation.settings.navigationAssemblyName);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Time: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.time.ToString("G4") + "/" + Simulation.settings.maximumTestTime);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Timescale (" + Simulation.timeScale.ToString("G2") + "): ", GUILayout.Width(leftWidth));
		Simulation.timeScale = GUILayout.HorizontalSlider(Simulation.timeScale, 0.5f, 4f);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Sim Number: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.simulationNumber + " of " + Simulation.batch.Count);
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Test Number: ", GUILayout.Width(leftWidth));
		GUILayout.Label(Simulation.testNumber + " of " + Simulation.settings.numberOfTests);
		GUILayout.EndHorizontal();
		
		bool randomDest = Simulation.settings.randomizeDestination;
		bool randomStart = Simulation.settings.randomizeOrigin;
		bool repeatOnComplete = Simulation.settings.continueOnNavObjectiveComplete;
		bool repeatOnStuck = Simulation.settings.continueOnRobotIsStuck;
		
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
		Simulation.settings.continueOnNavObjectiveComplete = repeatOnComplete;
		Simulation.settings.continueOnRobotIsStuck = repeatOnStuck;
		
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
				Simulation.End();
			if (GUILayout.Button("Next Test"))
				Simulation.NextTest(Simulation.StopCode.UserRequestNextTest);
		}
		if (Simulation.isFinished) {
			if  (GUILayout.Button("Start Again")) {
				Simulation.Begin();
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
	
}
