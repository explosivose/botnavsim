using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_Prompt : IWindowFunction {

	public enum Type {
		Close,
		YesNo,
		OkCancel
	}

	
/// <summary>
/// Initializes a new instance of the <see cref="UI_Prompt"/> class.
/// </summary>
/// <param name="response">Response callback function Reponse(bool).</param>
/// <param name="t">T.</param>
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
	
	public delegate void Response(bool response);
	
	private Response callback;
	private bool close;
	
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
