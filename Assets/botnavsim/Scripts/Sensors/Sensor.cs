using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract Sensor class for all sensor implementations to inherit from.
/// </summary>
abstract public class Sensor : MonoBehaviour {
	
	/// <summary>
	/// Enable this instance and pass the reference to the 
	/// Robot.SensorData callback function to be called when the 
	/// sensor has new information to share. 
	/// </summary>
	/// <param name="callback">Callback.</param>
	abstract public void Enable(Robot.SensorData callback);
	
	/// <summary>
	/// Disable this instance.
	/// </summary>
	abstract public void Disable();
}
