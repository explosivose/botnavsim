using UnityEngine;
using System.Collections;

/// <summary>
/// IToolbar class provides controls for the Robot Editor (not implemented!)
/// </summary>
public class UI_RobotEditor : IToolbar {

	public UI_RobotEditor() {
		hidden = true;
		priority = -1;
	}
	
	public bool contextual {
		get {
			return BotNavSim.isIdle ||
				BotNavSim.isEditingRobot;
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
			return "Robot Editor";
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
