using UnityEngine;
using System.Collections;

/// <summary>
/// IWindowFunction class for displaying credits. Also inherits from MonoBehaviour 
/// to expose credit data to UnityEditor when added as a component to the UI GameObject
/// </summary>
public class UI_Credits : MonoBehaviour, IWindowFunction {

	
	[System.Serializable]
	/// <summary>
	/// Serializable (exposed to UnityEditor) for holding creditation data
	/// </summary>
	public class Credit {
		/// <summary>
		/// Name of the contribution to credit.
		/// </summary>
		public string name;
		
		/// <summary>
		/// Name of the author of the contribution.
		/// </summary>
		public string author;
		
		/// <summary>
		/// Description of the contribution. 
		/// </summary>
		public string description;
		
		/// <summary>
		/// The URL link for more information. 
		/// </summary>
		public string url;
	}
	
	/// <summary>
	/// Array of credits exposed and edited in the Unity Inspector
	/// </summary>
	public Credit[] credits;
	
	/// <summary>
	/// The width of the window.
	/// </summary>
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
	
	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake() {
		windowRect = new Rect(0,0,500,500);
	}
	
	/// <summary>
	/// GUI Window function to display credits in a scroll view.
	/// </summary>
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
