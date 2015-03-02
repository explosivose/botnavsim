using UnityEngine;
using System.Collections;

public interface IToolbar {
	
	string toolbarName 	{ get; }
	bool contextual 	{ get; }
	bool hidden			{ get; }
	Rect rect			{ get; set; }
	GUI.WindowFunction 	ToolbarWindow(int windowID);
	
}
