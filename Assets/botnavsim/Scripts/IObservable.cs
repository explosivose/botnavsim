using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for objects that are observable via CamController must keep their world bounds up-to-date
/// </summary>
public interface IObservable {
	/// <summary>
	/// Gets the name of the observable object
	/// </summary>
	/// <value>The name.</value>
	string name {get;}
	
	/// <summary>
	/// Gets the bounds of the observable object (size and location)
	/// </summary>
	/// <value>The bounds.</value>
	Bounds bounds {get;}
}
