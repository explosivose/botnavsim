using UnityEngine;
using System.Collections;

public class UI_Credits : MonoBehaviour, IWindowFunction {

	[System.Serializable]
	public class Credit {
		public string name;
		public string author;
		public string description;
		public string url;
	}
	// public fields exposed to Unity Inspector
	public Credit[] credits;
	public float width = 500f;
	
	public string windowTitle {
		get {
			return "Credits";
		}
	}

	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
		get {
			if (_close) return null;
			return Window;
		}
	}
	
	private bool _close;
	private Vector2 _scrollPos = new Vector2();
	
	void Awake() {
		windowRect = new Rect(0,0,500,500);
	}
	
	void Window(int id) {
		if (GUILayout.Button("Close")) {
			_close = true;
		}
		_scrollPos = GUILayout.BeginScrollView(_scrollPos, true, false);
		GUILayout.Label(Strings.projectAbout, GUILayout.Width(width-60f));
		GUILayout.Space (25);
		GUILayout.Label("The following resources were used in this project: ");
		GUILayout.Space (25);
		foreach(Credit c in credits) {
			GUILayout.Label(c.name + " by " + c.author,GUILayout.Width(width-60f));
			GUILayout.Label(c.description,GUILayout.Width(width-60f));
			if (GUILayout.Button(c.url,GUILayout.Width(width-60f))) Application.OpenURL(c.url);
			GUILayout.Space (50);
		}
		GUILayout.EndScrollView();
		GUI.DragWindow();
	}
	
}
