using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// IToolbar class for displaying and editing the simulation batch list
/// </summary>
public class UI_SimulationBatch : IToolbar  {

	public UI_SimulationBatch() {
		_settings = new Simulation.Settings();
		_editSettings = new UI_SimulationSettings(_settings);
		_files = new List<string>();
		_folders = new List<string>();
		_windows = new Stack<GUI.WindowFunction>();
		_windows.Push(BatchListWindow);
		// hide window initially
		hidden = true;
	}

	public bool contextual {
		get {
			return BotNavSim.isIdle || 
				BotNavSim.isSimulating || 
				BotNavSim.isViewingData;
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

	public string currentDir {
		get {
			return Strings.logFileDirectory + _subPath;
		}
	}
			
	private Stack<GUI.WindowFunction> _windows;
	private UI_SimulationSettings _editSettings;
	private List<string> _files;
	private List<string> _folders;
	private string _subPath;
	private Simulation.Settings _settings;

	/// <summary>
	/// Refresh this the files and folders in current directory
	/// </summary>
	private void Refresh() {
		_files = FileBrowser.ListFiles(currentDir, "*.xml");
		_folders = FileBrowser.ListFolders(currentDir);
	}
	
	
	/// <summary>
	/// List Simulation.Settings in Simulation.batch and provide controls and editing the Simulation.batch
	/// </summary>
	void BatchListWindow(int windowID) {
		
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		
		// add a new simulation to batch
		if (GUILayout.Button("Add new simulation")) {
			_settings = new Simulation.Settings();
			_editSettings = new UI_SimulationSettings(_settings);
			UI_Toolbar.I.additionalWindows.Add((IWindowFunction)_editSettings);
		}
		// load a simulation from xml file
		if (GUILayout.Button("Add to batch from file...")) {
			Refresh();
			_windows.Push(XmlBrowser);
		}
		GUILayout.EndHorizontal();
		
		// Batch list
		GUILayout.Label("Currently in batch:");
		if (Simulation.batch.Count < 1) {
			GUILayout.Label("None");
		}
		
		for(int i = 0; i < Simulation.batch.Count; i++) {
			Simulation.Settings batchItem = Simulation.batch[i];
			// batch list table row by row
			GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
			// display batch position
			if (i+1 == Simulation.simulationNumber) {
				GUILayout.Label("->");
			}
			// label 
			string label = i+1 + ". " + batchItem.title + ", " + batchItem.time;
			if (batchItem.active) label += ", RUNNING";
			GUILayout.Label(label);
			// edit these settings
			if (GUILayout.Button("Edit")) {
				_settings = batchItem;
				_editSettings = new UI_SimulationSettings(_settings);
				UI_Toolbar.I.additionalWindows.Add((IWindowFunction)_editSettings);
			}
			if (GUILayout.Button("X")) {
				Simulation.batch.RemoveAt(i);
			}
			// indicate which have been executed already
			// if (batchItem.executed)
			
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Space (10);
		// start simulating 
		if (Simulation.batch.Count > 0) {
			if (GUILayout.Button("Start Batch")) {
				BotNavSim.state = BotNavSim.State.Simulating;
				Simulation.Begin();
			}
			GUILayout.Space(20);
			// remove all simulations from batch
			GUILayout.Space (20);
			if (GUILayout.Button("Clear Batch")) {
				Simulation.batch.Clear();
			}
		}
	}
	
	/// <summary>
	/// Browse for XML files to deserialize into Simulation.Settings
	/// </summary>
	void XmlBrowser(int windowID) {
		// back button
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		// refresh files and folders
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			Refresh();
		}
		GUILayout.EndHorizontal();
		
		// go up one directory
		if (_subPath != "") {
			if (GUILayout.Button("..")) {
				_subPath = Directory.GetParent(_subPath).Name;
				Refresh();
			}
		}
		// list subdirectories
		for (int i = 0; i < _folders.Count; i++) {
			// enter subdirectory
			if (GUILayout.Button(_folders[i])) {
				_subPath += "\\" + new DirectoryInfo(_folders[i]).Name;
				Refresh();
			}
		}
		// list files
		for (int i = 0; i < _files.Count; i++) {
			// try paths from file
			if (GUILayout.Button(_files[i])) {
				string path = currentDir + "\\" + _files[i];
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
