using UnityEngine;
using System.Collections;

/// <summary>
/// UI window for live simulation settings.
/// Provides controls for pausing, stopping or skipping the current test
/// Provides a toggle for exhibition mode
/// Provides a slider for simulation timescale
/// </summary>
public class UI_SimulationSettings : IToolbar {

	public string toolbarName {
		get {
			return "Simulation Settings";
		}
	}

	public bool contextual {
		get; private set;
	}

	public bool hidden {
		get; set; 
	}

	public Rect rect {
		get; set; 
	}
	
	public GUI.WindowFunction window {
		get {
			return ToolbarWindow;
		}
	}
	
	private bool _liveEditSettings;
	private string _robotGallerySelection;
	private string _environmentGallerySelection;
	private string _navListSelection;
	
	/// <summary>
	/// Simulation settings window function called by UI_Toolbar.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	public void ToolbarWindow (int windowID) {
		float lw = 200f;
		
		// simulation information
		GUILayout.BeginHorizontal();
		GUILayout.Label(Simulation.settings.title + "(" + Simulation.simulationNumber + "/" +
		                Simulation.batch.Count + ") " +  Simulation.settings.time, GUILayout.Width(lw));
		GUILayout.Label("Test " + Simulation.testNumber + "/" + Simulation.settings.numberOfTests);
		GUILayout.EndHorizontal();
		
		// exhbition mode tickbox
		GUILayout.BeginHorizontal();
		GUILayout.Label("Exhibition Mode: ", GUILayout.Width(lw));
		Simulation.exhibitionMode = GUILayout.Toggle(Simulation.exhibitionMode, "");
		GUILayout.EndHorizontal();
		
		// timescale slider 
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Timescale: ", GUILayout.Width(lw));
		Simulation.timeScale = GUILayout.HorizontalSlider(
			Simulation.timeScale,
			0.5f, 4f);
		GUILayout.EndHorizontal();
		
		// contextual control buttons
		if (Simulation.isRunning) {
			GUILayout.BeginHorizontal();
			if (Simulation.paused) {
				if (GUILayout.Button("Play"))
					Simulation.paused = false;
			}
			else {
				if (GUILayout.Button("Pause"))
					Simulation.paused = true;
			}
			if (GUILayout.Button("Stop")) {
				Simulation.exhibitionMode = false;
				Simulation.End();
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Next Test")) {
				Simulation.NextTest(Simulation.StopCode.UserRequestNextTest);
			}
			
		}
		if (Simulation.isFinished) {
			if  (GUILayout.Button("Start Again")) {
				Simulation.Begin();
			}
			if (GUILayout.Button("New Simulation...")) {
				Simulation.End();
			}
		}
		
		if (_liveEditSettings) {
			if (GUILayout.Button("Hide Settings")) {
				_liveEditSettings = false;
			}
			SimulationSettingsControls(Simulation.settings);
		}
		else {
			if (GUILayout.Button("Show Settings")) {
				_liveEditSettings = true;
			}
		}
		
		// finish up
		if (!Simulation.isRunning) contextual = false;
		else contextual = true;
		
		
		GUI.DragWindow();
	}
	
	void SimulationSettingsControls(Simulation.Settings settings) {
		
		float lw = 200f;
		string title = settings.title;
		string robotName = settings.robotName;
		if (_robotGallerySelection != null) {
			robotName = _robotGallerySelection;
			_robotGallerySelection = null;
		}
		string environmentName = settings.environmentName;
		if (_environmentGallerySelection != null) {
			environmentName = _environmentGallerySelection;
			_environmentGallerySelection = null;
		}
		string navigationAssemblyName = settings.navigationAssemblyName;
		if (_navListSelection != null) {
			navigationAssemblyName = _navListSelection;
			_navListSelection = null;
		}
		string numberOfTests = settings.numberOfTests.ToString();
		string testTime = settings.maximumTestTime.ToString();
		bool randomDest = settings.randomizeDestination;
		bool randomStart = settings.randomizeOrigin;
		bool repeatOnComplete = settings.continueOnNavObjectiveComplete;
		bool repeatOnStuck = settings.continueOnRobotIsStuck;
		
		if (settings.active) {
			GUILayout.Label(settings.title + "\n" + 
			                settings.robotName + "\n" +
			                settings.environmentName + "\n" + 
			                settings.navigationAssemblyName);
		}
		else {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Title", GUILayout.Width(lw));
			title = GUILayout.TextField(title);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of tests: ", GUILayout.Width(lw));
			numberOfTests = GUILayout.TextField(numberOfTests);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Robot selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(robotName)) {
				_window.Push(RobotGalleryWindow);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Environment selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(environmentName)) {
				_window.Push(EnvironmentGalleryWindow);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Algorithm selection: ", GUILayout.Width(lw));
			if (GUILayout.Button(navigationAssemblyName)) {
				_window.Push(NavListWindow);
			}
			GUILayout.EndHorizontal();
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Randomize Start: ", GUILayout.Width(lw));
		randomStart = GUILayout.Toggle(randomStart,"");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Randomize Destination: ", GUILayout.Width(lw));
		randomDest = GUILayout.Toggle(randomDest,"");
		GUILayout.EndHorizontal();
		
		if (settings.active) {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Time (s): " + Simulation.time.ToString("G2"), GUILayout.Width(lw));
			testTime = GUILayout.TextField(testTime);
			GUILayout.EndHorizontal();
		}
		else {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Maximum Simulation Time (s): ", GUILayout.Width(lw));
			testTime = GUILayout.TextField(testTime);
			GUILayout.EndHorizontal();
		}
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on complete: " , GUILayout.Width(lw));
		repeatOnComplete = GUILayout.Toggle(repeatOnComplete,"");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Repeat on stuck: ", GUILayout.Width(lw));
		repeatOnStuck = GUILayout.Toggle(repeatOnStuck, "");
		GUILayout.EndHorizontal();
		
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
		
		settings.robotName = robotName;
		settings.environmentName = environmentName;
		settings.navigationAssemblyName = navigationAssemblyName;
		settings.randomizeDestination = randomDest;
		settings.randomizeOrigin = randomStart;
		settings.continueOnNavObjectiveComplete = repeatOnComplete;
		settings.continueOnRobotIsStuck = repeatOnStuck;
	}
}
