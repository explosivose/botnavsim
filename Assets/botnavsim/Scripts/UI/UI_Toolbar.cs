using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Toolbar provides controls for choosing which UI windows to display
/// </summary>
public class UI_Toolbar : MonoBehaviour {

	public static UI_Toolbar I;
	
	public float height;
	
	private List<IToolbar> _tools = new List<IToolbar>();
	private GUISkin _skin;
	private int id;
	
	void Awake() {
		// singleton pattern
		if (I == null) {
			I = this;
		}
		else {
			Destroy(this);
		}
		// instantiate all classes that implement IToolbar
		Type ti = typeof(IToolbar);
		foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type t in asm.GetTypes()) {
				if (ti.IsAssignableFrom(t)) {
					_tools.Add(Activator.CreateInstance(t) as IToolbar);
				}
			}
		}
		// get GUISkin
		_skin = Resources.Load<GUISkin>("GUI_style");
	}
	
	/// <summary>
	/// Draw GUI elements
	/// </summary>
	void OnGUI() {
		GUI.skin = _skin;
		Rect rect = new Rect(0f,0f,Screen.width,height);
		id = 1;
		GUILayout.Window(id++, rect, ToolbarWindow, Strings.projectTitle);
		
	}
	
	/// <summary>
	/// Toolbar window GUI.WindowFunction
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ToolbarWindow(int windowID) {
		GUILayout.BeginHorizontal();
		foreach(IToolbar t in _tools) {
			// only handle windows that are contextual
			if (t.contextual) {
				// toggle window show/hide if button pressed
				if (GUILayout.Button(t.toolbarName)) {
					t.hidden = !t.hidden;
				}
				// display windows that aren't hidden
				if (!t.hidden) {
					GUILayout.Window(id++, t.rect, t.window, t.toolbarName);
				}
			}
		}
		GUILayout.EndHorizontal();
	}
}
