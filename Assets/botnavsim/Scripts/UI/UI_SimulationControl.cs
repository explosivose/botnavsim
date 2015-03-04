using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI window for simulation control. 
/// Provides controls for pausing, stopping or skipping the current test
/// Provides a toggle for exhibition mode.
/// Provides a slider for simulation timescale.
/// </summary>
public class UI_SimulationControl : IToolbar {


	public string toolbarName {
		get {
			return "Simulation Control";
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
		}
		else {
			if (GUILayout.Button("Show Settings")) {
				_editSettings = new UI_SimulationSettings(Simulation.settings);
				_liveEditSettings = true;
			}
		}
		
		if (_liveEditSettings) {
			if (_editSettings.completed) {
				_liveEditSettings = false;
			}
			else {
				_editSettings.rect = GUILayout.Window(123, _editSettings.rect, _editSettings.window, "Edit Live Settings");
			}
		}
		
		// finish up
		if (!Simulation.isRunning) contextual = false;
		else contextual = true;
		
		
		GUI.DragWindow();
	}
	

}
