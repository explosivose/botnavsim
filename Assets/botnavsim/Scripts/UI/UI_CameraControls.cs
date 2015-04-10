using UnityEngine;
using System.Collections;

[System.Serializable]
public class UI_CameraControls : IToolbar {

	public UI_CameraControls() {
		hidden = true;
	}
	
	public bool contextual {
		get {
			return !BotNavSim.isIdle;
		}
	}

	public bool hidden {
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
	
	void CameraControlsWindow(int windowID) {
		float lw = 200f;
		// close window button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("<", GUILayout.Width(30f))) {
			hidden = true;
		}
		GUILayout.EndHorizontal();
		
		// camera perspective controls
		GUILayout.BeginHorizontal();
		GUILayout.Label("Camera Perspective: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.viewMode.ToString())) {
			CamController.CycleViewMode();
		}
		GUILayout.EndHorizontal();
		
		// camera render mode controls
		GUILayout.BeginHorizontal();
		GUILayout.Label("Render Mode: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.renderMode.ToString())) {
			CamController.CycleRenderMode();
		}
		GUILayout.EndHorizontal();
		
		//GUI.DragWindow();
	} 
}
