using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI_setup : MonoBehaviour {

	private GUISkin _style;
	
	private Rect _rectMain;
	private Rect _rectChild;
	
	private Simulation.Settings _sim;
	
	
	private GUI.WindowFunction child;
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_style = Resources.Load<GUISkin>("GUI_style");
		
		_rectMain = new Rect();
		_rectChild = new Rect();
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
		
		_rectMain = GUILayout.Window(i++, _rectMain, SetupWindow, "Create Simulation");
		
		if (child != null) {
			_rectChild.y = _rectMain.y;
			_rectChild.x = _rectMain.x + _rectMain.width;
			_rectChild = GUILayout.Window(i++, _rectChild, child, "");
		}
	}
	
	void SetupWindow(int windowID) {
		
		float leftWidth = 150f;
		string robotName = Simulation.settings.robotName;
		string envName = Simulation.settings.environmentName;
		string algName = Simulation.settings.navigationAssemblyName;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Title: ", GUILayout.Width(leftWidth));
		Simulation.settings.title = GUILayout.TextField(Simulation.settings.title);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Robot: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(robotName)) {
			child = WindowRobotGallery;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Environment: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(envName)) {
			child = WindowEnvironmentGallery;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Algorithm: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(algName)) {
			child = WindowNavAlgorithms;
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
		
		if (Simulation.batch.Count > 0 ) {
			if (GUILayout.Button("View Batch List (" + Simulation.batch.Count + ")")) {
				child = WindowBatchList;
			}
			if (GUILayout.Button("START BATCH")) {
				Simulation.Begin();
			}
		}
		
		else if (Simulation.isReady) {
			if (GUILayout.Button("ADD TO BATCH")) {
				Simulation.batch.Add(Simulation.settings);
				Simulation.settings = new Simulation.Settings();
				child = WindowBatchList;
			}
			if (GUILayout.Button("START")) {
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
				child = null;
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
				child = null;
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
				child = null;
			}
		}

	}
	

	
	void WindowBatchList(int windowID) {
		foreach (Simulation.Settings sim in Simulation.batch) {
			if (GUILayout.Button(sim.title + ", " + sim.time)) {
				_sim = sim;
				child = WindowSimSummary;
			}
		}
		GUILayout.Space (10);
		if (GUILayout.Button("Clear Batch")) {
			Simulation.batch.Clear();
		}
	}
	
	void WindowSimSummary(int windowID) {
		
		GUILayout.Label(_sim.summary);
		if (GUILayout.Button("Remove from batch?")) {
			Simulation.batch.Remove(_sim);
			if (Simulation.batch.Count > 0 )
				child = WindowBatchList;
			else
				child = null;
		}
	}
}
