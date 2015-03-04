using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_SimulationBatch : IToolbar {

	public UI_SimulationBatch() {
		_editSettings = new UI_SimulationSettings(new Simulation.Settings());
		_simulationFiles = new List<string>();
		_windows.Push(BatchListWindow);
	}
	
	public string toolbarName {
		get {
			return "Simulation Batch";
		}
	}

	public bool contextual {
		get {
			return true;
		}
	}

	public bool hidden {
		get; set;
	}

	public Rect rect {
		get; set;
	}

	public GUI.WindowFunction window {
		get {
			return _windows.Peek();
		}
	}
	
	private Stack<GUI.WindowFunction> _windows;
	private UI_SimulationSettings _editSettings;
	private List<string> _simulationFiles;

	void BatchListWindow(int windowID) {

		// controls and title
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.Label("Batch List");
		GUILayout.EndHorizontal();
		
		for(int i = 0; i < Simulation.batch.Count; i++) {
			if (GUILayout.Button(Simulation.batch[i].title + ", " + Simulation.batch[i].time)) {
				_editSettings = new UI_SimulationSettings(Simulation.batch[i]);
			}
		}
		GUILayout.Space (10);
		if (GUILayout.Button("Start Batch")) {
			Simulation.Begin();
		}
		if (GUILayout.Button("Add to batch from file...")) {
			_simulationFiles = ObjectSerializer.SearchForObjects(Strings.simulationFileDirectory);
			_windows.Push(SimulationListWindow);
		}
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
