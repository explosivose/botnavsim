using UnityEngine;
using System.Collections;

public class UI_Credits : IWindowFunction {

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
	
	public UI_Credits() {
		windowRect = new Rect(0,0,500,500);
	}
	
	bool _close;
	Vector2 _scrollPos = new Vector2();
	float _width = 500f;
	
	void Window(int id) {
		if (GUILayout.Button("Close")) {
			_close = true;
		}
		_scrollPos = GUILayout.BeginScrollView(_scrollPos, true, false);
		GUILayout.Label(Strings.projectAbout, GUILayout.Width(_width-60f));
		GUILayout.Space (50);
		foreach(Strings.Credit c in Strings.projectCredits) {
			GUILayout.Label(c.name + " by " + c.author,GUILayout.Width(_width-60f));
			GUILayout.Label(c.description,GUILayout.Width(_width-60f));
			if (GUILayout.Button(c.url,GUILayout.Width(_width-60f))) Application.OpenURL(c.url);
			GUILayout.Space (50);
		}
		GUILayout.EndScrollView();
		GUI.DragWindow();
	}
}
