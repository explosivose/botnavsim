using UnityEngine;
using System.Collections;

public class UI_Simulation : MonoBehaviour {

	public UI_window windowSim = new UI_window();

	private GUISkin _style;
	private Rect _rectSim;
	private bool _hideMenu = true;
	private CameraPerspective _camPersp;
	private CameraType _camType;
	
	void Start() {
		_style = Resources.Load<GUISkin>("GUI_style");
		_rectSim = new Rect(windowSim.Left, windowSim.Top, windowSim.Width, windowSim.Height);
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
		if (Simulation.isRunning)
		_rectSim = GUILayout.Window(100, _rectSim, WindowControls, "Simulation Settings", _style.window);
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
		
		Simulation.autoRepeat = GUILayout.Toggle(Simulation.autoRepeat, "Auto Repeat");
		
		if (GUILayout.Button("Quit")) {
			Application.Quit();
		}
		
		GUILayout.Label("Simulation time: " + Simulation.time);
		GUILayout.Label(Simulation.botscript.description);
		
	}
}
