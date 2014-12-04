using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_setup : MonoBehaviour {

	public UI_window windowNavAlg = new UI_window();
	public UI_window windowRobot = new UI_window();
	public UI_window windowEnv = new UI_window();
	public UI_window windowSim = new UI_window();
	
	private GUISkin _style;
	
	private Rect _rectNavAlg;
	private bool _showNavAlg;
	
	private Rect _rectRobot;
	private bool _showRobot;
	
	private Rect _rectEnv;
	private bool _showEnv;
	
	private Rect _rectSim;

	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_style = Resources.Load<GUISkin>("GUI_style");
		_rectNavAlg = new Rect(windowNavAlg.Left, windowNavAlg.Top, windowNavAlg.Width, windowNavAlg.Height);
		_rectRobot = new Rect(windowRobot.Left, windowRobot.Top, windowRobot.Width, windowRobot.Height);
		_rectEnv = new Rect(windowEnv.Left, windowEnv.Top, windowEnv.Width, windowEnv.Height);
		_rectSim = new Rect(windowSim.Left, windowSim.Top, windowSim.Width, windowSim.Height);
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		NavLoader.SearchForPlugins();
		BotLoader.SearchForRobots();
	}
	
	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI() {
		int i = 1;
		_rectSim = GUILayout.Window(i++, _rectSim, WindowSimulationSettings, "Simulation Settings", _style.window);
		if (Simulation.isRunning) return;
		_rectNavAlg = GUILayout.Window(i++, _rectNavAlg, WindowNavAlgorithms, "Navigation Algorithm", _style.window);
		_rectRobot = GUILayout.Window(i++, _rectRobot, WindowRobotGallery, "Robot Gallery", _style.window);
		_rectEnv = GUILayout.Window(i++, _rectEnv, WindowEnvironmentGallery, "Environment Gallery", _style.window);
		
	}
	
	/// <summary>
	/// Navigation algorithm gallery window.
	/// </summary>
	void WindowNavAlgorithms(int windowID) {
		GUILayout.Label("Active Plugin: " + NavLoader.activePlugin, _style.label);
		
		// collapsed window
		if (!_showNavAlg) {
			if (GUILayout.Button("Show Navigation Algorithms", _style.button)) {
				_rectNavAlg.height = 0;
				_showNavAlg = true;
			}
			GUI.DragWindow();
			return;
		}
		
		// expanded window
		
		// collapse button
		if (GUILayout.Button("Hide Navigation Algorithms", _style.button))
			_showNavAlg = false;
			
		// refresh button
		if (GUILayout.Button("Refresh List", _style.button))
			NavLoader.SearchForPlugins();
		
		foreach(string s in NavLoader.pluginsFound) {
			if (GUILayout.Button(s, _style.button))
				NavLoader.SetPlugin(s);
		}
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Robot gallery window.
	/// </summary>
	void WindowRobotGallery(int windowID) {
		string robotName = "<NONE>";
		if (Simulation.robot) robotName = Simulation.robot.name;
		GUILayout.Label("Selected Robot: " + robotName, _style.label);
		
		// collapsed window
		if (!_showRobot) {
			if (GUILayout.Button("Show Robot Gallery", _style.button)) {
				_rectRobot.height = 0;
				_showRobot = true;
			}
			GUI.DragWindow();
			return;
		}
		
		// expanded window
		
		// collapse button
		if (GUILayout.Button("Hide Robot Gallery", _style.button)) 
			_showRobot = false;
		
		// start robot creator
		GUILayout.Button("Create new robot...");
		GUILayout.Space(10);
					
		// gallery goes here...
		
		for(int i = 0; i < BotLoader.robotsFound.Count; i++) 
			if (GUILayout.Button(BotLoader.robotsFound[i].name, _style.button))
				BotLoader.SetRobot(i);
		
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Environment gallery window.
	/// </summary>
	void WindowEnvironmentGallery(int windowID) {
		GUILayout.Label("Selected Environment: " + Application.loadedLevelName, _style.label);
		
		// collapsed window
		if (!_showEnv) {
			if (GUILayout.Button("Show Environment Gallery", _style.button)) {
				_rectEnv.height = 0;
				_showEnv = true;
			}
			GUI.DragWindow();
			return;
		}
		
		// expanded window
		
		// collapse button
		if (GUILayout.Button("Hide Environment Gallery", _style.button)) 
			_showEnv = false;
			
		// gallery goes here...
		
		for(int i = 0; i < Application.levelCount; i++)
			if (GUILayout.Button("Level " + i, _style.button))
				Application.LoadLevel(i);
			
		GUI.DragWindow();
	}
	
	void WindowSimulationSettings(int windowID) {
		if (Simulation.isReady) {
			if (GUILayout.Button("START", _style.button)) {
				Simulation.Run();
			}
			if (GUILayout.Button("STOP", _style.button)) {
				Simulation.Stop();
			}
		}
		else {
			GUILayout.Label("Select a robot and navigation algorithm.");
		}

	}
}
