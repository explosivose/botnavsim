using UnityEngine;
using System.Collections;

public interface INavigation {
	
	//# Search parameters

	/// <summary>
	/// Gets or sets the search space bounds.
	/// </summary>
	/// <value>Defines the search space boundaries in simulation world coordinates.</value>
	Bounds searchBounds		{get; set;}
	
	/// <summary>
	/// Gets or sets the origin.
	/// </summary>
	/// <value>The origin is the start of the path to the destination.</value>
	Vector3 origin 			{get; set;}
	
	/// <summary>
	/// Gets or sets the destination.
	/// </summary>
	/// <value>The destination is where the path leads to.</value>
	Vector3 destination 	{get; set;}
	
	//# Start search routine
	
	/// <summary>
	/// Start the search algorithm to find a path from <see cref="origin"/> to <see cref="destination"/>.
	/// </summary>
	/// <returns>An iterator which enables Unity to continue execution after calling this routine.</returns>
	IEnumerator SearchForPath();
	IEnumerator SearchForPath(Vector3 start, Vector3 end);
	
	/// <summary>
	/// Gets a value indicating whether this <see cref="INavigation"/> found a path between <see cref="origin"/> and <see cref="destination"/>.
	/// </summary>
	/// <value><c>true</c> if path found; otherwise, <c>false</c>.</value>
	bool pathFound 			{get;}
	
	//# Robot communication 
	
	/// <summary>
	/// Gives a direction to follow the path given your location.
	/// </summary>
	/// <returns>The path direction.</returns>
	/// <param name="myLocation">My location.</param>
	Vector3 PathDirection(Vector3 myLocation);
	
	/// <summary>
	/// Proximity sensor data.
	/// </summary>
	/// <param name="from">Sensor position.</param>
	/// <param name="to">Sensor reading position.</param>
	/// <param name="obstructed">If set to <c>true</c> position at <param name="to"/> is obstructed.</param>
	void Proximity(Vector3 from, Vector3 to, bool obstructed);
	
	//# Debugging
	
	/// <summary>
	/// Called by Unity OnDrawGizmos() event to draw 3D world space debug information.
	/// </summary>
	void DrawGizmos();
	
	void DrawDebugInfo();
	
	/* Some other methods that might be useful in future...
	void Obstruction(Vector3 location);
	
	void Unobstructed(Vector3 location);
	
	void ForgetObstructions();
	*/
}