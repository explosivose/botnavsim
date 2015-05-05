using UnityEngine;
using System.Collections;

/// <summary>
/// IToolbar class provides controls for the Environment Editor (not implemented!)
/// </summary>
public class UI_EnvironmentEditor : IToolbar {

	public UI_EnvironmentEditor() {
		hidden = true;
	}
	
	public bool contextual {
		get {
			return BotNavSim.isIdle ||
				BotNavSim.isEditingEnvironment;
		}
	}
	
	public bool hidden {
		get; set;
	}
	
	public string windowTitle {
		get {
			return "Environment Editor";
		}
	}
	
	public Rect windowRect {
		get; set;
	}
	
	public GUI.WindowFunction windowFunction {
		get {
			return EditorWindow;
		}
	}
	
	void EditorWindow(int windowID) {
		GUILayout.Label("Not yet implemented.");
	}
}
