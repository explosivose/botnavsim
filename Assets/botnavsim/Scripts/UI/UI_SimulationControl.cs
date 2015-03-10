using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI window for simulation control. 
/// Provides controls for pausing, stopping or skipping the current test
/// Provides a toggle for exhibition mode.
/// Provides a slider for simulation timescale.
/// </summary>
[System.Serializable]
public class UI_SimulationControl : IToolbar {

	public UI_SimulationControl() {
		_editSettings = new UI_SimulationSettings(Simulation.settings);
		hidden = true;
	}

	public bool contextual {
		get {
			return !Simulation.preSimulation;
		}
	}

	public bool hidden {
		get; set; 
	}

	public string windowTitle {
		get {
			return "Simulation Control";
		}
	}

	public Rect windowRect {
		get; set; 
	}
	
	public GUI.WindowFunction windowFunction {
		get {
			return MainWindow;
		}
	}
	
	
	private bool _liveEditSettings;
	private UI_SimulationSettings _editSettings;
	
	
	/// <summary>
	/// Simulation settings window function called by UI_Toolbar.
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void MainWindow (int windowID) {
		float lw = 200f;
		
		// close window button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.EndHorizontal();
		
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
		
		// show/hide button for edit settings window
		if (_liveEditSettings) {
			if (GUILayout.Button("Hide Settings")) {
				_liveEditSettings = false;
				UI_Toolbar.I.additionalWindows.Remove((IWindowFunction)_editSettings);
			}
		}
		else {
			if (GUILayout.Button("Show Settings")) {
				_editSettings.settings = Simulation.settings;
				_liveEditSettings = true;
				UI_Toolbar.I.additionalWindows.Add((IWindowFunction)_editSettings);
			}
		}
		
		// close window when completed
		if (_liveEditSettings) {
			if (_editSettings.completed) {
				_liveEditSettings = false;
				UI_Toolbar.I.additionalWindows.Remove((IWindowFunction)_editSettings);
			}
		}
	
		
		GUI.DragWindow();
	}
	

}
