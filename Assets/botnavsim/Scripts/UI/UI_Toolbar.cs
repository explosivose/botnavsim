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
	public List<IWindowFunction> additionalWindows = new List<IWindowFunction>();

	//private IToolbar[] _tools;
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
		// get all IToolbar components on this GameObject
		//_tools = gameObject.GetComponents(typeof(IToolbar)) as IToolbar[];
		//Debug.Log(_tools.Length);
		/*MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
		foreach(MonoBehaviour component in components) {
			if (component is IToolbar) {
				IToolbar toolbarItem = component as IToolbar;
				_tools.Add(toolbarItem);
			}
		}*/
		
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
				//if (ti.IsAssignableFrom(t)) 
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
		// display toolbar window
		GUILayout.Window(id++, rect, ToolbarWindow, Strings.projectTitle);
		// display any visible tool windows
		foreach(IToolbar t in _tools) {
			// only handle windows that are contextual
			if (t.contextual) {
				// display windows that aren't hidden
				if (!t.hidden) {
					t.windowRect = GUILayout.Window(id++, t.windowRect, t.windowFunction, t.windowTitle);
				}
			}
		}
		
		foreach(IWindowFunction w in additionalWindows) {
			w.windowRect = GUILayout.Window(id++, w.windowRect, w.windowFunction, w.windowTitle);
		}
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
				if (GUILayout.Button(t.windowTitle)) {
					t.hidden = !t.hidden;
				}
			}
		}
		GUILayout.EndHorizontal();
	}
}
