using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for classes that implement a toolbar item
/// </summary>
public interface IToolbar : IWindowFunction {
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="IToolbar"/> is contextual.
	/// </summary>
	/// <value><c>true</c> if contextual; otherwise, <c>false</c>.</value>
	bool contextual 	{ get; }
	
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="IToolbar"/> is hidden.
	/// </summary>
	/// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
	bool hidden			{ get; set; }
	
}

/// <summary>
/// Interface for classes that implement a GUI.WindowFunction
/// </summary>
public interface IWindowFunction {
	
	/// <summary>
	/// Gets the window title.
	/// </summary>
	/// <value>The window title.</value>
	string windowTitle	{ get; }
	
	/// <summary>
	/// Gets or sets the window rect (window size and position).
	/// </summary>
	/// <value>The window rect.</value>
	Rect windowRect		{ get; set; }
	
	/// <summary>
	/// Gets the window function definition for GUILayout.Window()
	/// </summary>
	/// <value>The window function.</value>
	GUI.WindowFunction	windowFunction { get; }
}
