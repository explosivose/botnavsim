using UnityEngine;
using System.Collections;

/// <summary>
/// IToolbar class for providing controls of data logging.
/// Includes editing parameters to log and enable/disable logging. 
/// Calls functions to Log class
/// </summary>
public class UI_Log : IToolbar  {

	public UI_Log() {
		hidden = true;
		priority = 0;
	}
	
	public bool contextual {
		get {
			return BotNavSim.isIdle || 
				BotNavSim.isSimulating;
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
	
	/// <summary>
	/// GUI Window function provides controls for logging data. 
	/// </summary>
	void LogSettingsWindow(int windowID) {
		float w = UI_Toolbar.I.innerWidth/2f;
		
		// close window button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.EndHorizontal();
		
		// toggle logging
		GUILayout.BeginHorizontal();
		GUILayout.Label("Log to file: ", GUILayout.Width(w));
		Simulation.loggingEnabled = GUILayout.Toggle(Simulation.loggingEnabled, "");
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Available Parameters", GUILayout.Width(w));
		GUILayout.Label("Parameters To Be Logged", GUILayout.Width(w));
		GUILayout.EndHorizontal();
		
		int max = Mathf.Max(Log.availableParams.Count, Log.selectedParams.Count);
		
		for(int i = 0; i < max; i++) {
			GUILayout.BeginHorizontal();
			if (i < Log.availableParams.Count) {
				if (GUILayout.Button(Log.availableParams[i].ToString(), GUILayout.Width(w))) {
					Log.LogParameter(Log.availableParams[i], true);
				}
			}
			else {
				GUILayout.Button("", GUILayout.Width(w));
			}
			if (i < Log.selectedParams.Count) {
				if(GUILayout.Button(Log.selectedParams[i].ToString(),GUILayout.Width(w))) {
					Log.LogParameter(Log.selectedParams[i], false);
				}
			}
			else {
				GUILayout.Button("", GUILayout.Width(w));
			}
			GUILayout.EndHorizontal();
		}
		
	}
}
