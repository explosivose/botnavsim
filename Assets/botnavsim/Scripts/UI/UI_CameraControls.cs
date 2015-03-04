using UnityEngine;
using System.Collections;

public class UI_CameraControls : IToolbar {

	public string toolbarName {
		get {
			return "Camera Controls";
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
		if (GUILayout.Button(CamController.Instance.perspective.ToString())) {
			CamController.Instance.CyclePerspective();
		}
		GUILayout.EndHorizontal();
		
		// camera render mode controls
		GUILayout.BeginHorizontal();
		GUILayout.Label("Render Mode: ", GUILayout.Width(lw));
		if (GUILayout.Button(CamController.Instance.renderMode.ToString())) {
			CamController.Instance.CycleRenderMode();
		}
		GUILayout.EndHorizontal();
	} 
}
