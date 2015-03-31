using UnityEngine;
using System.Collections;

/// <summary>
/// UI for the robot editor (not yet implemented)
/// </summary>
public class UI_RobotEditor : IToolbar {

	public UI_RobotEditor() {
		hidden = true;
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
