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
	
	public Rect rect {
		get; private set;
	}
	
	public float innerWidth {
		get {
			return rect.width - _skin.window.padding.horizontal - _skin.window.border.horizontal - 20;
		}
	}
	
	private List<IToolbar> _tools = new List<IToolbar>();
	private GUISkin _skin;
	private Vector2 _scrollPos;
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
		_scrollPos = new Vector2();
		
	}
	
	/// <summary>
	/// Draw GUI elements
	/// </summary>
	void OnGUI() {
		// set skin object
		GUI.skin = _skin;
		winId = 1;
		// set toolbar size and position
		rect = new Rect(Screen.width-width,0,width,Screen.height);
		// display toolbar window
		rect = GUILayout.Window(winId++, rect, ToolbarWindow, Strings.projectTitle + "-" + Strings.projectVersion);

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
		_scrollPos = GUILayout.BeginScrollView(_scrollPos, true, false);
		// horizontal separator
		GUILayout.Box("", GUILayout.Width(innerWidth), GUILayout.Height(5));
		if (!BotNavSim.isIdle) {
			if (GUILayout.Button("Back", GUILayout.Width(innerWidth))) {
				BotNavSim.state = BotNavSim.State.Idle;
			}
			// horizontal separator
			GUILayout.Box("", GUILayout.Width(innerWidth), GUILayout.Height(5));
		}
		foreach(IToolbar t in _tools) {
			// only handle windows that are contextual
			if (t.contextual) {
				// toggle window show/hide if button pressed
				if (GUILayout.Button(t.windowTitle, GUILayout.Width(innerWidth))) {
					t.hidden = !t.hidden;
				}
				if (!t.hidden) t.windowFunction(0);
				// horizontal separator
				GUILayout.Box("", GUILayout.Width(innerWidth), GUILayout.Height(5));
			}
		}
		GUILayout.EndScrollView();
	}
}
