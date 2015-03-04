using UnityEngine;
using System.Collections;

public class UI_Log : IToolbar {

	public string toolbarName {
		get {
			return "Log Settings";
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
			return LogSettingsWindow;
		}
	}
	
	void LogSettingsWindow(int windowID) {
		float lw = 200f;
		
		// close window button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.EndHorizontal();
		
		// toggle logging
		GUILayout.BeginHorizontal();
		GUILayout.Label("Log to file: ", GUILayout.Width(lw));
		Simulation.loggingEnabled = GUILayout.Toggle(Simulation.loggingEnabled, "");
		GUILayout.EndHorizontal();

		lw = rect.width/2f;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Available Parameters", GUILayout.Width(lw));
		GUILayout.Label("Parameters To Be Logged");
		GUILayout.EndHorizontal();
		
		int max = Mathf.Max(Log.availableParams.Count, Log.selectedParams.Count);
		
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
		
		GUI.DragWindow();
	}
}
