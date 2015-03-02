using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Toolbar provides controls for choosing which UI windows to display
/// </summary>
public class UI_Toolbar : MonoBehaviour {

	public float height;
	
	private List<IToolbar> _tools = new List<IToolbar>();
	private GUISkin _skin;
	
	void Awake() {
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
	
	void OnGUI() {
		
		Rect rect = new Rect(0f,0f,Screen.width,height);
		int id = 1;
		GUILayout.Window(id++, rect, ToolbarWindow, Strings.projectTitle);
	}
	
	void ToolbarWindow(int windowID) {
		GUILayout.BeginHorizontal();
		foreach(IToolbar t in _tools) {
			if (t.contextual) {
				if (GUILayout.Button(t.toolbarName)) {
					t.hidden = !t.hidden;
				}
			}
		}
		GUILayout.EndHorizontal();
	}
}
