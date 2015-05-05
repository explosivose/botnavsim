using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// IWindowFunction class that prompts the user for action.
/// </summary>
public class UI_Prompt : IWindowFunction {

	
	/// <summary>
	/// Response callback delegate type.
	/// </summary>
	public delegate void Response(bool response);

	public enum Type {
		/// <summary>
		/// Close type response: display only a "Close" button.
		/// </summary>
		Close,
		
		/// <summary>
		/// Yes or No type response: display "Yes" and "No" buttons.
		/// </summary>
		YesNo,
		
		/// <summary>
		/// Ok or Cancel type response: display "Ok" and "Cancel" buttons.
		/// </summary>
		OkCancel
	}

	
/// <summary>
/// Initializes a new instance of the <see cref="UI_Prompt"/> class.
/// </summary>
/// <param name="response">Response callback function Reponse(bool).</param>
/// <param name="t">Response Type.</param>
/// <param name="title">Title.</param>
/// <param name="prompt">Prompt.</param>
	public UI_Prompt(
		Response response, 
		Type t, 
		string title = "Prompt", 
		string prompt = "Keep up the good work!") {
		
		callback = response;
		type = t;
		windowTitle = title;
		message = prompt;
		
		windowRect = new Rect(Screen.width/2f, Screen.height/2f, 400, 100);
	}

	public Type type {
		get; set;
	}
	
	public string windowTitle {
		get; set;
	}

	public string message {
		get; set;
	}

	public Rect windowRect {
		get; set;
	}

	public GUI.WindowFunction windowFunction {
		get {
			if (close) return null;
			else return Window;
		}
	}

	
	/// <summary>
	/// The callback function to call once the user has responded to the prompt.
	/// </summary>
	private Response callback;
	private bool close;
	
	/// <summary>
	/// GUI window function for the prompt.
	/// </summary>
	void Window(int windowID) {
		
		GUILayout.Label(message);
				
		switch(type) {
			case Type.Close:
				if (GUILayout.Button("Close")) {
				  	close = true;
					callback(false);
				}
			break;
			
			case Type.OkCancel:
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("OK")) {
					callback(true);
					close = true;
				}
				GUILayout.Space(20f);
				if (GUILayout.Button("Cancel")) {
					callback(false);
					close = true;
				}
				GUILayout.EndHorizontal();
			break;
			
			case Type.YesNo:
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Yes")) {
					callback(true);
					close = true;
				}
				GUILayout.Space(20f);
				if (GUILayout.Button("No")) {
					callback(false);
					close = true;
				}
				GUILayout.EndHorizontal();
			break;
		}
		
		GUI.DragWindow();
	}
	
	
}
