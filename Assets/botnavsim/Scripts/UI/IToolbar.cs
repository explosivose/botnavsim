using UnityEngine;
using System.Collections;

public interface IToolbar {
	
	string toolbarName 	{ get; }
	bool contextual 	{ get; }
	bool hidden			{ get; set; }
	Rect rect			{ get; set; }
	GUI.WindowFunction window { get; }
	
}
