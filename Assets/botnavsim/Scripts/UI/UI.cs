using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI : MonoBehaviour {

	private Stack<GUI.WindowFunction> _window;
	private GUISkin _skin;
	private Rect _rect;
	private Simulation.Settings _tempSim;
	private string 	_navListSelection;
	private string 	_robotGallerySelection;
	private string 	_environmentGallerySelection;
	private int _batchIndex;
	private bool _liveEditSettings;

	void Awake() {
		_skin = Resources.Load<GUISkin>("GUI_style");
		_rect = new Rect();
		_tempSim = new Simulation.Settings();
		_window = new Stack<GUI.WindowFunction>();
	}

	void Start() {
		NavLoader.SearchForPlugins();
		BotLoader.SearchForRobots();
		EnvLoader.SearchForEnvironments();
		_navListSelection = "<none>";
		_robotGallerySelection = "<none>";
		_environmentGallerySelection = "<none>";
		_window.Push(SetupWindow);
	}

	void OnGUI() {
		GUI.skin = _skin;
		_rect.height = 0f;
		_rect = GUILayout.Window(0, _rect, _window.Peek(), Strings.projectTitle + " " + Strings.projectVersion);

	}

	void WindowHeader() {
		if (GUILayout.Button("Exit Application")) {
			Application.Quit();
		}
		GUILayout.Space (10);
	}

	void SetupWindow(int windowID) {
		
		float lw = 200f;
		
		WindowHeader();

		// controls and title
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Setup");
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.Label("Log to file: ", GUILayout.Width(lw));
		Simulation.loggingEnabled = GUILayout.Toggle(Simulation.loggingEnabled, "");
		if (GUILayout.Button("Edit log list")) {
			_window.Push(LogListWindow);
		}
		GUILayout.EndHorizontal();
	
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Exhibition Mode: ", GUILayout.Width(lw));
		Simulation.exhibitionMode = GUILayout.Toggle(Simulation.exhibitionMode, "");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Timescale: ", GUILayout.Width(lw));
		Simulation.timeScale = GUILayout.HorizontalSlider(
			Simulation.timeScale,
			0.5f, 4f);
		GUILayout.EndHorizontal();
		
		SimulationSettingsControls(_tempSim);
				
		if (Simulation.batch.Count > 0 ) {
			if (GUILayout.Button("View Batch List (" + Simulation.batch.Count + ")")) {
				_window.Push(BatchListWindow);
			}
			if (_tempSim.isValid) {
				if (GUILayout.Button("ADD TO BATCH")) {
					Simulation.batch.Add(_tempSim);
					_tempSim = new Simulation.Settings();
					_window.Push(BatchListWindow);
				}
			}
			if (GUILayout.Button("START BATCH")) {
				_window.Push(SimulationWindow);
				Simulation.Begin();
			}
		}
		
		else if (_tempSim.isValid) {
			if (GUILayout.Button("ADD TO BATCH")) {
				Simulation.batch.Add(_tempSim);
				_tempSim = new Simulation.Settings();
				_window.Push(BatchListWindow);
			}
			if (GUILayout.Button("START")) {
				Simulation.batch.Add(_tempSim);
				_window.Push(SimulationWindow);
				Simulation.Begin();
			}
		}
		else {
			GUILayout.Button("Not ready.");
		}
		
		
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

	void SimulationWindow(int windowID) {
		float lw = 200f;
		
		WindowHeader();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label(Simulation.settings.title + ", " + Simulation.settings.time, GUILayout.Width(lw));
		GUILayout.Label(Simulation.testNumber + "/" + Simulation.settings.numberOfTests);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Log to file: ", GUILayout.Width(lw));
		Simulation.loggingEnabled = GUILayout.Toggle(Simulation.loggingEnabled, "");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Exhibition Mode: ", GUILayout.Width(lw));
		Simulation.exhibitionMode = GUILayout.Toggle(Simulation.exhibitionMode, "");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Timescale: ", GUILayout.Width(lw));
		Simulation.timeScale = GUILayout.HorizontalSlider(
			Simulation.timeScale,
			0.5f, 4f);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Camera Perspective: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.Instance.perspective.ToString())) {
			CamController.Instance.CyclePerspective();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Render Mode: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.Instance.renderMode.ToString())) {
			CamController.Instance.CycleRenderMode();
		}
		GUILayout.EndHorizontal();
		
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
				Simulation.End();
				_window.Pop();
			}
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Next Test")) {
				Simulation.NextTest(Simulation.StopCode.UserRequestNextTest);
				_window.Pop();
			}
			
		}
		if (Simulation.isFinished) {
			if  (GUILayout.Button("Start Again")) {
				Simulation.Begin();
			}
			if (GUILayout.Button("New Simulation...")) {
				Simulation.state = Simulation.State.preSimulation;
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
		
		GUI.DragWindow();
	}

	/// <summary>
	/// Navigation algorithm gallery window.
	/// </summary>
	void NavListWindow(int windowID) {
		WindowHeader();

		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			NavLoader.SearchForPlugins();
		}
		GUILayout.Label("Nav Algorithm List");
		GUILayout.EndHorizontal();
		
		foreach(string s in NavLoader.pluginsFound) {
			if (GUILayout.Button(s)) {
				_navListSelection = s;
				_window.Pop();
			}
		}
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Robot gallery window.
	/// </summary>
	void RobotGalleryWindow(int windowID) {
		WindowHeader();
		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			BotLoader.SearchForRobots();
		}
		GUILayout.Label("Robot Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < BotLoader.robotsFound.Count; i++) {
			if (GUILayout.Button(BotLoader.robotsFound[i].name)) {
				_robotGallerySelection = BotLoader.robotsFound[i].name;
				_window.Pop();
			}
		}
		
		GUILayout.Space (10);
		
		// start robot creator
		GUILayout.Button("Create new robot...");
		GUI.DragWindow();
	}
	
	/// <summary>
	/// Environment gallery window.
	/// </summary>
	void EnvironmentGalleryWindow(int windowID) {
		WindowHeader();
		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			EnvLoader.SearchForEnvironments();
		}
		GUILayout.Label("Environment Gallery");
		GUILayout.EndHorizontal();
		
		// gallery goes here...
		for(int i = 0; i < EnvLoader.environmentsFound.Count; i++) {
			if (GUILayout.Button(EnvLoader.environmentsFound[i].name)) {
				_environmentGallerySelection = EnvLoader.environmentsFound[i].name;
				_window.Pop();
			}
		}
		
		GUILayout.Space(10);
		
		// start environment creator
		GUILayout.Button("Create new environment...");
		GUI.DragWindow();
	}
	
	
	
	void BatchListWindow(int windowID) {
		WindowHeader();
		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		GUILayout.Label("Batch List");
		GUILayout.EndHorizontal();

		for(int i = 0; i < Simulation.batch.Count; i++) {
			Simulation.Settings sim = Simulation.batch[i];
			if (GUILayout.Button(sim.title + ", " + sim.time)) {
				_batchIndex = i;
				_window.Push(SimSummaryWindow);
			}
		}
		GUILayout.Space (10);
		if (GUILayout.Button("Start Batch")) {
			_window.Push(SimulationWindow);
			Simulation.Begin();
		}
		GUILayout.Space (20);
		if (GUILayout.Button("Clear Batch")) {
			Simulation.batch.Clear();
		}
		GUI.DragWindow();
	}
	
	void SimSummaryWindow(int windowID) {
		WindowHeader();
		// back button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		GUILayout.Label("Edit Simulation In Batch");
		GUILayout.EndHorizontal();

		SimulationSettingsControls(Simulation.batch[_batchIndex]);

		if (GUILayout.Button("Remove from batch?")) {
			Simulation.batch.RemoveAt(_batchIndex);
			if (Simulation.batch.Count > 0 )
				_window.Push(BatchListWindow);
			else
				_window.Pop();
		}
		GUI.DragWindow();
	}

	void LogListWindow(int windowID) {
		WindowHeader();
		// back button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_window.Pop();
		}
		GUILayout.Label("Edit Log Parameters");
		GUILayout.EndHorizontal();
		
		float lw = _rect.width/2f;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Available Parameters", GUILayout.Width(lw));
		GUILayout.Label("Parameters To Be Logged");
		GUILayout.EndHorizontal();
		
		int max = Math.Max(Log.availableParams.Count, Log.selectedParams.Count);
		
		for(int i = 0; i < max; i++) {
			GUILayout.BeginHorizontal();
			if (i < Log.availableParams.Count) {
				if (GUILayout.Button(Log.availableParams[i].ToString(), GUILayout.Width(lw))) {
					Log.LogParameter(Log.availableParams[i], true);
				}
			}
			else {
				GUILayout.Button("", GUILayout.Width(lw));
			}
			if (i < Log.selectedParams.Count) {
				if(GUILayout.Button(Log.selectedParams[i].ToString())) {
					Log.LogParameter(Log.selectedParams[i], false);
				}
			}
			else {
				GUILayout.Button("");
			}
			GUILayout.EndHorizontal();
		}
	}
}
