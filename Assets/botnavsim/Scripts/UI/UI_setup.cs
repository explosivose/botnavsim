using UnityEngine;
using System;
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
		EnvLoader.SearchForEnvironments();
	}
	
	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI() {
		if (!Simulation.preSimulation) return;
		GUI.skin = _style;
		int i = 1;
		
		_rectSim = GUILayout.Window(i++, _rectSim, SetupWindow, "Create Simulation");
		
		
		
		if (_showNavAlg) {
			_rectNavAlg.y = _rectSim.y;
			_rectNavAlg.x = _rectSim.x + _rectSim.width;
			_rectNavAlg = GUILayout.Window(i++, _rectNavAlg, WindowNavAlgorithms, "Navigation Algorithm");
		}
		
		if (_showRobot) {
			_rectRobot.y = _rectSim.y;
			_rectRobot.x = _rectSim.x + _rectSim.width;
			_rectRobot = GUILayout.Window(i++, _rectRobot, WindowRobotGallery, "Robot Gallery");
		}
		
		if (_showEnv) {
			_rectEnv.y = _rectSim.y;
			_rectEnv.x = _rectSim.x + _rectSim.width;
			_rectEnv = GUILayout.Window(i++, _rectEnv, WindowEnvironmentGallery, "Environment Gallery");
		}
		
		
	}
	
	void SetupWindow(int windowID) {
		
		float leftWidth = 150f;
		string robotName = Simulation.settings.robotName;
		string envName = Simulation.settings.environmentName;
		string algName = Simulation.settings.navigationAssemblyName;
			
		GUILayout.BeginHorizontal();
		GUILayout.Label("Robot: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(robotName)) {
			_showRobot = !_showRobot;
			_showEnv = false;
			_showNavAlg = false;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Environment: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(envName)) {
			_showEnv = !_showEnv;
			_showRobot = false;
			_showNavAlg = false;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Algorithm: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(algName)) {
			_showNavAlg = !_showNavAlg;
			_showRobot = false;
			_showEnv = false;
		}
		GUILayout.EndHorizontal();
		
		string numberOfTests = Simulation.settings.numberOfTests.ToString();
		bool randomDest = Simulation.settings.randomizeDestination;
		bool randomStart = Simulation.settings.randomizeOrigin;
		bool repeatOnComplete = Simulation.settings.continueOnNavObjectiveComplete;
		bool repeatOnStuck = Simulation.settings.continueOnRobotIsStuck;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Number of tests: ", GUILayout.Width(leftWidth));
		numberOfTests = GUILayout.TextField(numberOfTests.ToString());
		GUILayout.EndHorizontal();
		
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
		
		if (Strings.IsDigitsOnly(numberOfTests)) {
			try {
				Simulation.settings.numberOfTests = Convert.ToInt32(numberOfTests);
			}
			catch {
				Debug.Log("User should enter a number...");
			}
		}
			
		Simulation.settings.randomizeDestination = randomDest;
		Simulation.settings.randomizeOrigin = randomStart;
		Simulation.settings.continueOnNavObjectiveComplete = repeatOnComplete;
		Simulation.settings.continueOnRobotIsStuck = repeatOnStuck;
		
		if (Simulation.isReady) {
			if (GUILayout.Button("START", _style.button)) {
				Simulation.Begin();
			}
		}
		else {
			GUILayout.Button("Not ready.");
		}
		
		
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Navigation algorithm gallery window.
	/// </summary>
	void WindowNavAlgorithms(int windowID) {
		// refresh button
		if (GUILayout.Button("Refresh List", _style.button))
			NavLoader.SearchForPlugins();
		
		foreach(string s in NavLoader.pluginsFound) {
			if (GUILayout.Button(s, _style.button)) {
				Simulation.settings.navigationAssemblyName = s;
				_showNavAlg = false;
			}
				
		}
		
	}
	
	/// <summary>
	/// Robot gallery window.
	/// </summary>
	void WindowRobotGallery(int windowID) {
		// start robot creator
		GUILayout.Button("Create new robot...");
		GUILayout.Space(10);
					
		// gallery goes here...
		
		for(int i = 0; i < BotLoader.robotsFound.Count; i++) {
			if (GUILayout.Button(BotLoader.robotsFound[i].name, _style.button)) {
				Simulation.settings.robotName = BotLoader.robotsFound[i].name;
				_showRobot = false;
			}
		}

	}
	
	/// <summary>
	/// Environment gallery window.
	/// </summary>
	void WindowEnvironmentGallery(int windowID) {
		GUILayout.Button("Create new environment...");
		GUILayout.Space(10);
		
		// gallery goes here...
		
		for(int i = 0; i < EnvLoader.environmentsFound.Count; i++) {
			if (GUILayout.Button(EnvLoader.environmentsFound[i].name, _style.button)) {
				Simulation.settings.environmentName = EnvLoader.environmentsFound[i].name;
				_showEnv = false;
			}
		}

	}
	
	
}
