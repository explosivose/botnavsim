using UnityEngine;
using System.Collections;



abstract public class Sensor : MonoBehaviour {

	abstract public ProximityData GetData();
	
}
