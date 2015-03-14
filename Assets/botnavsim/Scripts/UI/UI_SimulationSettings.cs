using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An instantiable class that provides a UI for editing a given Simulation.Settings object
/// </summary>
[System.Serializable]
public class UI_SimulationSettings : IWindowFunction {

	private Stack<GUI.WindowFunction> _windows;
	private Simulation.Settings _settings;
	
	public UI_SimulationSettings(Simulation.Settings simSettings) {
		_windows = new Stack<GUI.WindowFunction>();
		_windows.Push(SimulationSettingsWindow);
		_settings = simSettings;
	}
	
	public Simulation.Settings settings {
		get {
			return _settings;
		}
	}
	
	public string windowTitle {
		get {
			string title = "<null>";
			if (settings != null) 
				title = settings.title;
			
			return "Edit: " + title;
		}
	}
	
	/// <summary>
	/// Edit Simulation.Settings window function.
	/// Note that when the back button is pressed on the main window
	/// this will empty the stack and this window property will return null.
	/// To check whether the stack is empty see completed flag property.
	/// </summary>
	/// <value>The window function.</value>
	public GUI.WindowFunction windowFunction {
		get {
			if (_windows.Count > 0)
				return _windows.Peek();
			else
				return null;
		}	
	}
	
	/// <summary>
	/// Window size and position.
	/// </summary>
	/// <value>The rect.</value>
	public Rect windowRect {
		get; set;
	}
	
	
	/// <summary>
	/// Edit Simulation.Settings window function (always top of _windows stack)
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void SimulationSettingsWindow(int windowID) {


		GUILayout.BeginHorizontal();

		if (!_settings.active) {
			// if this settings is already batched
			if (Simulation.batch.Contains(_settings)) {
				// button to remove from batch
				if (GUILayout.Button("Remove from batch")) {
					Simulation.batch.Remove(_settings);
					_windows.Pop();
					return;
				}
				// cancel button
				if (GUILayout.Button("Close window")) {
					_windows.Pop();
					return;
				}
			}
			// this settings isn't in the batch
			else {
				// button to add valid settings to batch
				if (_settings.isValid) {
					if (GUILayout.Button("Add to batch")) {
						Simulation.batch.Add(_settings);
						_windows.Pop();
						return;
					}
				}
				// cancel button
				if (GUILayout.Button("Cancel")) {
					_windows.Pop();
					return;
				}
			}
		}
		else {
			GUILayout.Button("Simulation running!");
			if (GUILayout.Button("Close")) {
				_windows.Pop();
				return;
			}
		}

		GUILayout.EndHorizontal();
		
		float lw = 200f;
		
		// copy settings for UI
		string title = settings.title;
		string robotName = settings.robotName;
		string environmentName = settings.environmentName;
		string navigationAssemblyName = settings.navigationAssemblyName;
		string numberOfTests = settings.numberOfTests.ToString();
		string testTime = settings.maximumTestTime.ToString();
		bool randomDest = settings.randomizeDestination;
		bool randomStart = settings.randomizeOrigin;
		bool repeatOnComplete = settings.continueOnNavObjectiveComplete;
		bool repeatOnStuck = settings.continueOnRobotIsStuck;
		
		// if settings is in use by Simulation
		if (settings.active) {
			// display only
			GUILayout.Label(settings.title + "\n" + 
			                settings.robotName + "\n" +
			                settings.environmentName + "\n" + 
			                settings.navigationAssemblyName);
		}
		else {
			// provide controls for editing
			
			// edit title
			GUILayout.BeginHorizontal();
			GUILayout.Label("Title", GUILayout.Width(lw));
			title = GUILayout.TextField(title);
			GUILayout.EndHorizontal();
			
			// edit number of tests
			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of tests: ", GUILayout.Width(lw));
			numberOfTests = GUILayout.TextField(numberOfTests);
			GUILayout.EndHorizontal();
			
			// change the robot
			GUILayout.BeginHorizontal();
			GUILayout.Label("Robot selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(robotName)) {
				BotLoader.SearchForRobots();
				_windows.Push(RobotGalleryWindow);
			}
			GUILayout.EndHorizontal();
			
			// change the environment
			GUILayout.BeginHorizontal();
			GUILayout.Label("Environment selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(environmentName)) {
				EnvLoader.SearchForEnvironments();
				_windows.Push(EnvironmentGalleryWindow);
			}
			GUILayout.EndHorizontal();
			
			// change the navigation assembly
			GUILayout.BeginHorizontal();
			GUILayout.Label("Algorithm selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(navigationAssemblyName)) {
				NavLoader.SearchForPlugins();
				_windows.Push(NavListWindow);
			}
			GUILayout.EndHorizontal();
		}
		
		// toggle random start position
		GUILayout.BeginHorizontal();
		GUILayout.Label("Randomize Start: ", GUILayout.Width(lw));
		randomStart = GUILayout.Toggle(randomStart,"");
		GUILayout.EndHorizontal();
		
		// toggle random destination position
		GUILayout.BeginHorizontal();
		GUILayout.Label("Randomize Destination: ", GUILayout.Width(lw));
		randomDest = GUILayout.Toggle(randomDest,"");
		GUILayout.EndHorizontal();
		
		// if settings is in use by Simulation
		if (settings.active) {
			// display time remaining and edit maximum test time
			GUILayout.BeginHorizontal();
			GUILayout.Label("Time (s): " + Simulation.time.ToString("G2"), GUILayout.Width(lw));
			testTime = GUILayout.TextField(testTime);
			GUILayout.EndHorizontal();
		}
		else {
			// edit maximum test time
			GUILayout.BeginHorizontal();
			GUILayout.Label("Maximum Simulation Time (s): ", GUILayout.Width(lw));
			testTime = GUILayout.TextField(testTime);
			GUILayout.EndHorizontal();
		}
		
		// edit toggle for automatically starting a new test
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on complete: " , GUILayout.Width(lw));
		repeatOnComplete = GUILayout.Toggle(repeatOnComplete,"");
		GUILayout.EndHorizontal();
		
		// edit toggle for stuck detection
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on stuck: ", GUILayout.Width(lw));
		repeatOnStuck = GUILayout.Toggle(repeatOnStuck, "");
		GUILayout.EndHorizontal();
		
		// check for valid data input before copying back to settings object
		bool valid = true;
		foreach(char c in Strings.invalidFileNameChars) {
			if (title.Contains(c.ToString())) valid = false;
		}
		if (valid) settings.title = title;
		
		if (Strings.IsDigitsOnly(numberOfTests)) {
			try {
				settings.numberOfTests = Convert.ToInt32(numberOfTests);
			}
			catch {
				Debug.Log("User should enter a number...");
			}
		}
		
		if (Strings.IsDigitsOnly(testTime)) {
			try {
				settings.maximumTestTime = Convert.ToInt32(testTime);
			}
			catch {
				Debug.Log("User should enter a number...");
			}
		}
		
		// copy valid data back to settings
		settings.robotName = robotName;
		settings.environmentName = environmentName;
		settings.navigationAssemblyName = navigationAssemblyName;
		settings.randomizeDestination = randomDest;
		settings.randomizeOrigin = randomStart;
		settings.continueOnNavObjectiveComplete = repeatOnComplete;
		settings.continueOnRobotIsStuck = repeatOnStuck;
		
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Environment gallery window.
	/// </summary>
	void EnvironmentGalleryWindow(int windowID) {
		// back button and refresh button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			EnvLoader.SearchForEnvironments();
		}
		GUILayout.Label("Environment Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < EnvLoader.environmentsFound.Count; i++) {
			if (GUILayout.Button(EnvLoader.environmentsFound[i].name)) {
				settings.environmentName = EnvLoader.environmentsFound[i].name;
				_windows.Pop();
			}
		}
		
		GUILayout.Space(10);
		
		// start environment creator
		GUILayout.Button("Create new environment...");
		
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Robot gallery window.
	/// </summary>
	void RobotGalleryWindow(int windowID) {
		// back button and refresh button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			BotLoader.SearchForRobots();
		}
		GUILayout.Label("Robot Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < BotLoader.robotsFound.Count; i++) {
			if (GUILayout.Button(BotLoader.robotsFound[i].name)) {
				settings.robotName = BotLoader.robotsFound[i].name;
				_windows.Pop();
			}
		}
		
		GUILayout.Space (10);
		
		// start robot creator
		GUILayout.Button("Create new robot...");
		
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Navigation algorithm gallery window.
	/// </summary>
	void NavListWindow(int windowID) {
		// back button and refresh button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			NavLoader.SearchForPlugins();
		}
		GUILayout.Label("Nav Algorithm List");
		GUILayout.EndHorizontal();
		
		// gallery 
		foreach(string s in NavLoader.pluginsFound) {
			if (GUILayout.Button(s)) {
				settings.navigationAssemblyName = s;
				_windows.Pop();
			}
		}
		
		GUI.DragWindow();
	}
}
