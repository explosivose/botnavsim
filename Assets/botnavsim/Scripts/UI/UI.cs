using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UI : MonoBehaviour {

	private Stack<GUI.WindowFunction> _window;
	private GUISkin _skin;
	private Rect _rect;
	private Simulation.Settings _tempSim;
	private string _navListSelection;
	private string _robotGallerySelection;
	private string _environmentGallerySelection;
	private int _batchIndex;

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
		_rect = GUILayout.Window(0, _rect, _window.Peek(), Strings.projectTitle + " " + Strings.projectVersion);

	}

	void WindowHeader() {
		if (GUILayout.Button("Exit Application")) {
			Application.Quit();
		}
		GUILayout.Space (10);
	}

	void SetupWindow(int windowID) {
		
		WindowHeader();

		// controls and title
		GUILayout.BeginHorizontal();
		GUILayout.Label("Simulation Setup");
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

		float leftWidth = 200f;
		string title = settings.title;
				
		GUILayout.BeginHorizontal();
		GUILayout.Label("Title", GUILayout.Width(leftWidth));
		title = GUILayout.TextField(title);
		GUILayout.EndHorizontal();
		
		bool valid = true;
		foreach(char c in Strings.invalidFileNameChars) {
			if (title.Contains(c.ToString())) valid = false;
		}
		if (valid) settings.title = title;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Robot selection: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(_robotGallerySelection)) {
			_window.Push(RobotGalleryWindow);
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Environment selection: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(_environmentGallerySelection)) {
			_window.Push(EnvironmentGalleryWindow);
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Algorithm selection: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(_navListSelection)) {
			_window.Push(NavListWindow);
		}
		GUILayout.EndHorizontal();
		
		
		
		string numberOfTests = settings.numberOfTests.ToString();
		string testTime = settings.maximumTestTime.ToString();
		float timeScale = settings.initialTimeScale;
		bool randomDest = settings.randomizeDestination;
		bool randomStart = settings.randomizeOrigin;
		bool repeatOnComplete = settings.continueOnNavObjectiveComplete;
		bool repeatOnStuck = settings.continueOnRobotIsStuck;
		
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

		settings.robotName = _robotGallerySelection;
		settings.environmentName = _environmentGallerySelection;
		settings.navigationAssemblyName = _navListSelection;
		settings.initialTimeScale = timeScale;
		settings.randomizeDestination = randomDest;
		settings.randomizeOrigin = randomStart;
		settings.continueOnNavObjectiveComplete = repeatOnComplete;
		settings.continueOnRobotIsStuck = repeatOnStuck;
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
				//_tempSim.navigationAssemblyName = s;
				_navListSelection = s;
				_window.Pop();
			}
		}
		
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
				//_tempSim.robotName = BotLoader.robotsFound[i].name;
				_robotGallerySelection = BotLoader.robotsFound[i].name;
				_window.Pop();
			}
		}
		
		GUILayout.Space (10);
		
		// start robot creator
		GUILayout.Button("Create new robot...");
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
				//_tempSim.environmentName = EnvLoader.environmentsFound[i].name;
				_environmentGallerySelection = EnvLoader.environmentsFound[i].name;
				_window.Pop();
			}
		}
		
		GUILayout.Space(10);
		
		// start environment creator
		GUILayout.Button("Create new environment...");
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
		if (GUILayout.Button("Clear Batch")) {
			Simulation.batch.Clear();
		}
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
	}

	void SimulationWindow(int windowID) {
		float leftWidth = 150f;
		
		WindowHeader();
		
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
		/*
		GUILayout.BeginHorizontal();
		GUILayout.Label("Camera: ", GUILayout.Width(leftWidth));
		if (GUILayout.Button(_camPersp.perspective.ToString() + "/" + _camType.type.ToString())) {
			_showCameraMenu = true;
		}
		GUILayout.EndHorizontal();
		*/
		if (Simulation.isRunning) {
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
	}
}
