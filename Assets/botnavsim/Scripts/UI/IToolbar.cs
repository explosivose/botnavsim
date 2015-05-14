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
	
	int priority		{ get; set; }
}
