using UnityEngine;
using System.Collections;

/// <summary>
/// Propeller model. Fields are exposed to Unity Inspector
/// </summary>
[System.Serializable]
public class Propeller {
	public Transform tr;
	public Rigidbody rb;
	public float maxThrust;
	public Pid controller;
}
