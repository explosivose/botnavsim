using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI Toolbar provides controls for choosing which UI windows to display
/// </summary>
public class UI_Toolbar : MonoBehaviour {

	/// <summary>
	/// Singleton pattern.
	/// </summary>
	public static UI_Toolbar I;
	
	/// <summary>
	/// The width of the toolbar in screen px.
	/// </summary>
	public float width;
	
	/// <summary>
	/// The additional windows to be drawn outside the toolbar.
	/// used for classes like UI_SimulationSettings
	/// </summary>
	public List<IWindowFunction> additionalWindows = new List<IWindowFunction>();

	private List<IToolbar> _tools = new List<IToolbar>();
	private GUISkin _skin;
	private int winId;
	
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
			Type[] types;
			try {
				types = asm.GetTypes();
			}
			catch(ReflectionTypeLoadException e) {
				foreach (Exception le in e.LoaderExceptions) {
					Debug.LogException(le);
				}
				types = e.Types;
			}
			foreach(Type t in types) {
				if (t == null) continue;
				Type toolbarType = t.GetInterface(ti.FullName);
				if (toolbarType != null) {
					_tools.Add((IToolbar)Activator.CreateInstance(t));
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
		// set skin object
		GUI.skin = _skin;
		// set toolbar size and position
		Rect rect = new Rect(0,0,width,Screen.height);
		winId = 1;
		// display toolbar window
		GUILayout.Window(winId++, rect, ToolbarWindow, Strings.projectTitle + "-" + Strings.projectVersion);
		// display any visible toolbar windows
		foreach(IToolbar t in _tools) {
			// only handle windows that are contextual
			if (t.contextual) {
				// display windows that aren't hidden
				if (!t.hidden) {
					t.windowRect = GUILayout.Window(winId++, t.windowRect, t.windowFunction, t.windowTitle);
				}
			}
		}
		// display any additional windows
		for(int i = 0; i < additionalWindows.Count; i++) {
			IWindowFunction w = additionalWindows[i];
			if (w.windowFunction == null) {
				Debug.LogWarning("Null window removed.");
				additionalWindows.Remove(w);
			} else {
				w.windowRect = GUILayout.Window(winId++, w.windowRect, w.windowFunction, w.windowTitle);
			}
		}
	}
	
	/// <summary>
	/// Toolbar window GUI.WindowFunction (a list of buttons for showing/hiding tools)
	/// </summary>
	/// <param name="windowID">Window ID.</param>
	void ToolbarWindow(int windowID) {
		GUILayout.BeginVertical();
		if (!BotNavSim.isIdle) {
			if (GUILayout.Button("Back")) {
				BotNavSim.state = BotNavSim.State.Idle;
			}
		}
		foreach(IToolbar t in _tools) {
			// only handle windows that are contextual
			if (t.contextual) {
				// toggle window show/hide if button pressed
				if (GUILayout.Button(t.windowTitle)) {
					t.hidden = !t.hidden;
				}
			}
		}
		GUILayout.EndVertical();
	}
}
