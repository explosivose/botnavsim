using UnityEngine;
using System.Collections;

[System.Serializable]
public class UI_Log : IToolbar  {

	public UI_Log() {
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
			return "Log Settings";
		}
	}

	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
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

		lw = windowRect.width/2f;
		
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
