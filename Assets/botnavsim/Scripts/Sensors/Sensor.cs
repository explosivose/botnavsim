using UnityEngine;
using System.Collections;



abstract public class Sensor : MonoBehaviour {
	abstract public void Enable(Robot.SensorData callback);
	abstract public void Disable();
}
