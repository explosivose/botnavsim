using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// IToolbar class provides controls for loading and viewing CSV data logs. 
/// </summary>
public class UI_LogLoader : IToolbar {

	public UI_LogLoader() {
		if (!Directory.Exists(Strings.logFileDirectory))
			Directory.CreateDirectory(Strings.logFileDirectory);
		// initialise file browsing
		_files = FileBrowser.ListFiles(Strings.logFileDirectory);
		_folders = FileBrowser.ListFolders(Strings.logFileDirectory);
		_subPath = "\\";
		// initialise window stack
		_windows = new Stack<GUI.WindowFunction>();
		_windows.Push(Legend);
		hidden = true;
		priority = 10;
	}
	
	public bool contextual {
		get {
			return BotNavSim.isIdle || 
				BotNavSim.isViewingData;
		}
	}

	public bool hidden {
		get; set;
	}

	public int priority {
		get; set;
	}

	public string windowTitle {
		get {
			return "Log Loader";
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
	private List<string> _files;
	private List<string> _folders;
	private string _subPath;
	
	/// <summary>
	/// Refresh the files and folders in current directory
	/// </summary>
	private void Refresh() {
		_files = FileBrowser.ListFiles(currentDir, "*.csv");
		_folders = FileBrowser.ListFolders(currentDir);
	}
	
	/// <summary>
	/// Determines whether the mouse was hovering over the last drawn rect object
	/// </summary>
	/// <returns><c>true</c> if mouse over; otherwise, <c>false</c>.</returns>
	private bool IsMouseOver() {
		return Event.current.type == EventType.Repaint &&
			GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
	}
	
	/// <summary>
	/// GUI window function: browse and load CSV files 
	/// </summary>
	private void CsvBrowser(int windowID) {
		// hide button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			_windows.Pop();
		}
		// refresh files and folders
		if (GUILayout.Button("R", GUILayout.Width(30f))) {
			Refresh();
		}
		GUILayout.EndHorizontal();
		
		// go up one directory
		if (_subPath != "\\") {
			if (GUILayout.Button("..")) {
				_subPath = Directory.GetParent(_subPath).Name;
				Refresh();
			}
		}
		// list subdirectories
		for (int i = 0; i < _folders.Count; i++) {
			// enter subdirectory
			if (GUILayout.Button(_folders[i])) {
				_subPath += new DirectoryInfo(_folders[i]).Name;
				Refresh();
			}
		}
		if (LogLoader.loading) {
			GUILayout.Label("Loading...");
		}
		else {
			// list files
			for (int i = 0; i < _files.Count; i++) {
				// try paths from file
				if (GUILayout.Button(_files[i])) {
					// change state when loading first data
					if (BotNavSim.isIdle) {
						BotNavSim.state = BotNavSim.State.ViewingData;
					}
					LogLoader.LoadPaths(currentDir + "\\" + _files[i]);
				}
			}
		}

	}
	
	/// <summary>
	/// GUI window function: Display controls for loaded CSV files. 
	/// </summary>
	private void Legend(int windowID) {
		// back button
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		if (GUILayout.Button("Load from CSV...")) {
			_windows.Push(CsvBrowser);
		}
		GUILayout.EndHorizontal();
		
		if (LogLoader.paths.Count < 1) {
			GUILayout.Label("No paths loaded.");
		}
		
		// list botpaths
		Color c = GUI.contentColor;
		for(int i = 0; i < LogLoader.paths.Count; i++) {
			GUILayout.BeginHorizontal();
			GUI.contentColor = LogLoader.paths[i].color;
			if (GUILayout.Button(LogLoader.paths[i].csvName)) {
				LogLoader.paths[i].visible = !LogLoader.paths[i].visible;
				continue;
			}
			// highlight path if mouseover button
			// this event appears to be broken 
			if (IsMouseOver()) {
				LogLoader.paths[i].highlight = true;
			} else {
				LogLoader.paths[i].highlight = false;
			}
			// observe button
			if (GUILayout.Button("O")) {
				CamController.SetAreaOfInterest(LogLoader.paths[i]);
			}
			// unload path button
			if (GUILayout.Button("X")) {
				LogLoader.RemovePath(LogLoader.paths[i]);
			}
			GUILayout.EndHorizontal();
		}
		// reset content color
		GUI.contentColor = c;

	}
	
	
}
