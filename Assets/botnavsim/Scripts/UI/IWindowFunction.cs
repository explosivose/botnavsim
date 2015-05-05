using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for classes that implement a GUI.WindowFunction
/// </summary>
public interface IWindowFunction {
	
	/// <summary>
	/// Gets the window title.
	/// </summary>
	string windowTitle	{ get; }
	
	/// <summary>
	/// Gets or sets the window rect (window size and position).
	/// </summary>
	Rect windowRect		{ get; set; }
	
	/// <summary>
	/// Gets the window function definition for GUILayout.Window()
	/// </summary>
	GUI.WindowFunction	windowFunction { get; }
}
