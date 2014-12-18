using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI_setup : MonoBehaviour {

	private GUISkin _style;
	
	private Rect _rectMain;
	private Rect _rectChild;
	
	private Simulation.Settings _tempSim;
	private int _batchIndex;
	
	private GUI.WindowFunction child;
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		_style = Resources.Load<GUISkin>("GUI_style");
		_rectMain = new Rect();
		_rectChild = new Rect();
		_tempSim = new Simulation.Settings();
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
		
		_rectMain = GUILayout.Window(i++, _rectMain, SetupWindow, Strings.projectTitle + " " + Strings.projectVersion);
		
		if (child != null) {
			_rectChild.y = _rectMain.y;
			_rectChild.x = _rectMain.x + _rectMain.width;
			_rectChild = GUILayout.Window(i++, _rectChild, child, "");
		}
	}
	
	void SetupWindow(int windowID) {
		
		float leftWidth = 200f;
		string title = _tempSim.title;
		string robotName = _tempSim.robotName;
		string envName = _tempSim.environmentName;
		string algName = _tempSim.navigationAssemblyName;
		
		if (GUILayout.Button("Exit Application")) {
			Application.Quit();
		}
		
		GUILayout.Space (10);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Title: ", GUILayout.Width(leftWidth));
		title = GUILayout.TextField(title);
		GUILayout.EndHorizontal();
		
		bool valid = true;
		foreach(char c in Strings.invalidFileNameChars) {
			if (title.Contains(c.ToString())) valid = false;
		}
		if (valid) _tempSim.title = title;
		
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
		

		
		string numberOfTests = _tempSim.numberOfTests.ToString();
		string testTime = _tempSim.maximumTestTime.ToString();
		float timeScale = _tempSim.initialTimeScale;
		bool randomDest = _tempSim.randomizeDestination;
		bool randomStart = _tempSim.randomizeOrigin;
		bool repeatOnComplete = _tempSim.continueOnNavObjectiveComplete;
		bool repeatOnStuck = _tempSim.continueOnRobotIsStuck;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Number of tests: ", GUILayout.Width(leftWidth));
		numberOfTests = GUILayout.TextField(numberOfTests);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Maximum test time (s): ", GUILayout.Width(leftWidth));
		testTime = GUILayout.TextField(testTime);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Initial timescale (" + timeScale.ToString("G2") + "): ", GUILayout.Width(leftWidth));
		timeScale = GUILayout.HorizontalSlider(timeScale, 0.5f, 4f);
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
				_tempSim.numberOfTests = Convert.ToInt32(numberOfTests);
			}
			catch {
				Debug.Log("User should enter a number...");
			}
		}
		
		if (Strings.IsDigitsOnly(testTime)) {
			try {
				_tempSim.maximumTestTime = Convert.ToInt32(testTime);
			}
			catch {
				Debug.Log("User should enter a number...");
			}
		}
		
		_tempSim.initialTimeScale = timeScale;
		_tempSim.randomizeDestination = randomDest;
		_tempSim.randomizeOrigin = randomStart;
		_tempSim.continueOnNavObjectiveComplete = repeatOnComplete;
		_tempSim.continueOnRobotIsStuck = repeatOnStuck;
		
		if (Simulation.batch.Count > 0 ) {
			if (GUILayout.Button("View Batch List (" + Simulation.batch.Count + ")")) {
				child = WindowBatchList;
			}
			if (_tempSim.isValid) {
				if (GUILayout.Button("ADD TO BATCH")) {
					Simulation.batch.Add(_tempSim);
					_tempSim = new Simulation.Settings();
					child = WindowBatchList;
				}
			}
			if (GUILayout.Button("START BATCH")) {
				Simulation.Begin();
			}
		}
		
		else if (_tempSim.isValid) {
			if (GUILayout.Button("ADD TO BATCH")) {
				Simulation.batch.Add(_tempSim);
				_tempSim = new Simulation.Settings();
				child = WindowBatchList;
			}
			if (GUILayout.Button("START")) {
				Simulation.batch.Add(_tempSim);
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
		
		// refresh button and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			NavLoader.SearchForPlugins();
		}
		GUILayout.Label("Nav Algorithm List");
		GUILayout.EndHorizontal();
		
		foreach(string s in NavLoader.pluginsFound) {
			if (GUILayout.Button(s, _style.button)) {
				_tempSim.navigationAssemblyName = s;
				child = null;
			}
		}
		
	}
	
	/// <summary>
	/// Robot gallery window.
	/// </summary>
	void WindowRobotGallery(int windowID) {
		
		// refresh button and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			BotLoader.SearchForRobots();
		}
		GUILayout.Label("Robot Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < BotLoader.robotsFound.Count; i++) {
			if (GUILayout.Button(BotLoader.robotsFound[i].name, _style.button)) {
				_tempSim.robotName = BotLoader.robotsFound[i].name;
				child = null;
			}
		}
		
		GUILayout.Space (10);
		
		// start robot creator
		GUILayout.Button("Create new robot...");
	}
	
	/// <summary>
	/// Environment gallery window.
	/// </summary>
	void WindowEnvironmentGallery(int windowID) {
		
		// refresh button and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			EnvLoader.SearchForEnvironments();
		}
		GUILayout.Label("Environment Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < EnvLoader.environmentsFound.Count; i++) {
			if (GUILayout.Button(EnvLoader.environmentsFound[i].name, _style.button)) {
				_tempSim.environmentName = EnvLoader.environmentsFound[i].name;
				child = null;
			}
		}

		GUILayout.Space(10);
		
		// start environment creator
		GUILayout.Button("Create new environment...");
	}
	

	
	void WindowBatchList(int windowID) {
		
		GUILayout.Label("Batch List");
		
		for(int i = 0; i < Simulation.batch.Count; i++) {
			Simulation.Settings sim = Simulation.batch[i];
			if (GUILayout.Button(sim.title + ", " + sim.time)) {
				_batchIndex = i;
				child = WindowSimSummary;
			}
		}
		GUILayout.Space (10);
		if (GUILayout.Button("Clear Batch")) {
			Simulation.batch.Clear();
		}
	}
	
	void WindowSimSummary(int windowID) {
		Simulation.Settings sim = Simulation.batch[_batchIndex];
		GUILayout.Label(sim.title + "\n" + sim.date + " " + sim.time);
		GUILayout.Space(10);
		GUILayout.Label(sim.summary);
		GUILayout.Space(10);
		if (GUILayout.Button("Remove from batch?")) {
			Simulation.batch.RemoveAt(_batchIndex);
			if (Simulation.batch.Count > 0 )
				child = WindowBatchList;
			else
				child = null;
		}
	}
}
