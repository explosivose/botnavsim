using UnityEngine;
using System.Collections;

/// <summary>
/// IToolbar class for controlling the camera. 
/// </summary>
public class UI_CameraControls : IToolbar {

	public UI_CameraControls() {
		hidden = true;
		priority = 20;
	}
	
	public bool contextual {
		get {
			return !BotNavSim.isIdle;
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
			return "Camera Controls";
		}
	}
	
	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
		get {
			return CameraControlsWindow;
		}
	}
	
	/// <summary>
	/// Provide controls for calling CamController functions
	/// </summary>
	void CameraControlsWindow(int windowID) {
		float lw = 200f;
		
		// camera perspective controls
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		GUILayout.Label("Camera Perspective: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.viewMode.ToString())) {
			CamController.CycleViewMode();
		}
		GUILayout.EndHorizontal();
		
		// camera render mode controls
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		GUILayout.Label("Render Mode: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.renderMode.ToString())) {
			CamController.CycleRenderMode();
		}
		GUILayout.EndHorizontal();
		
		// camera areas of interest
		GUILayout.BeginHorizontal(GUILayout.Width(UI_Toolbar.I.innerWidth));
		GUILayout.Label("Observing: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.area.name)) {
			CamController.CycleAreaOfInterest();
		}
		GUILayout.EndHorizontal();
	} 
}
