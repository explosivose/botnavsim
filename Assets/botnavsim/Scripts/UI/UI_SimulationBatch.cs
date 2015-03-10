using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vexe.Runtime.Types;
using Vexe;

/// <summary>
/// UI for displaying and editing the simulation batch list
/// </summary>
[System.Serializable]
public class UI_SimulationBatch : IToolbar  {

	public UI_SimulationBatch() {
		_settings = new Simulation.Settings();
		_editSettings = new UI_SimulationSettings(_settings);
		_simulationFiles = new List<string>();
		_windows = new Stack<GUI.WindowFunction>();
		_windows.Push(BatchListWindow);
		hidden = true;
	}

	public bool contextual {
		get {
			return true;
		}
	}

	public bool hidden {
		get; set;
	}
	
	public string windowTitle {
		get {
			return "Simulation Batch";
		}
	}

	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
		get {
			return _windows.Peek();
		}
	}
	
	private Stack<GUI.WindowFunction> _windows;
	private UI_SimulationSettings _editSettings;
	private List<string> _simulationFiles;
	private Simulation.Settings _settings;
	private bool _showEditSettings;

	void BatchListWindow(int windowID) {

		// remove editSettings window when completed
		if (_showEditSettings) {
			Debug.Log("showEditSettings");
			if (_editSettings.windowFunction == null) {
				_showEditSettings = false;
				UI_Toolbar.I.additionalWindows.Remove((IWindowFunction)_editSettings);
				if (_settings.isValid) {
					Simulation.batch.Add(_settings);
				}
				Debug.Log("Edit settings completed. Remove window");
			}
		}
		
		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.EndHorizontal();
		
		for(int i = 0; i < Simulation.batch.Count; i++) {
			if (GUILayout.Button(Simulation.batch[i].title + ", " + Simulation.batch[i].time)) {
				_settings = Simulation.batch[i];
				_showEditSettings = true;
				UI_Toolbar.I.additionalWindows.Add((IWindowFunction)_editSettings);
			}
		}
		GUILayout.Space (10);
		// start simulating 
		if (GUILayout.Button("Start Batch")) {
			Simulation.Begin();
		}
		GUILayout.Space(20);
		// add a new simulation to batch
		if (GUILayout.Button("Add new simulation")) {
			_settings = new Simulation.Settings();
			_editSettings.settings = _settings;
			_showEditSettings = true;
			UI_Toolbar.I.additionalWindows.Add((IWindowFunction)_editSettings);
		}
		// load a simulation from xml file
		if (GUILayout.Button("Add to batch from file...")) {
			_simulationFiles = ObjectSerializer.SearchForObjects(Strings.simulationFileDirectory);
			_windows.Push(SimulationListWindow);
		}
		// remove all simulations from batch
		GUILayout.Space (20);
		if (GUILayout.Button("Clear Batch")) {
			Simulation.batch.Clear();
		}
		

		
		GUI.DragWindow();
	}
	
	void SimulationListWindow(int windowID) {
		// back button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			_simulationFiles = ObjectSerializer.SearchForObjects(Strings.simulationFileDirectory);
		}
		GUILayout.EndHorizontal();
		
		for (int i = 0; i < _simulationFiles.Count; i++) {
			if (GUILayout.Button(_simulationFiles[i])) {
				string path = Strings.simulationFileDirectory + "\\";
				path += _simulationFiles[i] + ".xml";
				Simulation.Settings settings = ObjectSerializer.DeSerializeObject<Simulation.Settings>(path);
				if (settings != null) {
					settings.active = false;
					Simulation.batch.Add(settings);
					_windows.Pop();
				}
			}
		}
	}
}
